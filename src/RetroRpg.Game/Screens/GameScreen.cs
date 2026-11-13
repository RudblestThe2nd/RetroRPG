using System.Drawing.Drawing2D;

namespace RetroRpg.Game.Screens;

// Bütün ekranların tabanı. Ekranlar Windows Forms kontrolleri kullanmaz; her şey 800 x 800 alana
// asset görselleriyle çizilir. Düğmelerin fare durumlarını (hover, pressed) bu sınıf yönetir.
internal abstract class GameScreen : Control
{
    protected static readonly Rectangle CanvasRectangle = new(0, 0, MainGameForm.ScreenSize, MainGameForm.ScreenSize);
    protected static readonly Size CanvasSize = new(MainGameForm.ScreenSize, MainGameForm.ScreenSize);
    private readonly List<PixelButton> _buttons = [];
    private readonly List<PixelButton> _modalButtons = [];
    private PixelButton? _pressed;

    protected GameScreen(MainGameForm host)
    {
        Host = host;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.Background;
        Cursor = host.GameCursor;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
    }

    protected MainGameForm Host { get; }
    protected UiImages Images => Host.Images;

    // Açık bir pencere (örneğin zafer kutusu) varken yalnız onun düğmeleri tıklanabilir.
    protected bool ModalOpen { get; set; }

    public virtual void OnShown() => Focus();
    public virtual void HandleKey(Keys key) { }
    public virtual void HandleChar(char character) { }

    protected PixelButton AddButton(PixelButton button)
    {
        _buttons.Add(button);
        return button;
    }

    protected void ClearButtons() => _buttons.Clear();

    protected PixelButton AddModalButton(PixelButton button)
    {
        _modalButtons.Add(button);
        return button;
    }

    protected string AssetPath(string id) => Host.Assets.Entries.TryGetValue(id, out string? path) ? path : id;

    protected Point ToLogical(Point physical)
    {
        float scale = CanvasScale();
        float offsetX = (ClientSize.Width - MainGameForm.ScreenSize * scale) / 2f;
        float offsetY = (ClientSize.Height - MainGameForm.ScreenSize * scale) / 2f;
        return new Point((int)((physical.X - offsetX) / scale), (int)((physical.Y - offsetY) / scale));
    }

    private float CanvasScale() => Math.Max(0.01f,
        Math.Min((float)Math.Max(1, ClientSize.Width) / MainGameForm.ScreenSize,
                 (float)Math.Max(1, ClientSize.Height) / MainGameForm.ScreenSize));

    protected abstract void Draw(Graphics g);
    protected virtual void DrawModal(Graphics g) { }
    protected virtual void DrawTop(Graphics g) { }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(UiTheme.Background);
        float scale = CanvasScale();
        float offsetX = (ClientSize.Width - MainGameForm.ScreenSize * scale) / 2f;
        float offsetY = (ClientSize.Height - MainGameForm.ScreenSize * scale) / 2f;
        GraphicsState state = g.Save();
        using var transform = new Matrix(scale, 0, 0, scale, offsetX, offsetY);
        g.Transform = transform;
        g.SetClip(CanvasRectangle);
        UiTheme.Prepare(g);
        Draw(g);
        foreach (PixelButton button in _buttons.Where(x => x.Visible)) UiTheme.DrawButton(g, Images, button);
        DrawTop(g);
        if (ModalOpen)
        {
            DrawModal(g);
            foreach (PixelButton button in _modalButtons.Where(x => x.Visible)) UiTheme.DrawButton(g, Images, button);
        }
        UiTheme.DrawMissingAssets(g, Images.Missing, CanvasSize);
        g.Restore(state);
    }

    private IEnumerable<PixelButton> ActiveButtons => ModalOpen ? _modalButtons : _buttons;

    protected PixelButton? ButtonAt(Point point)
    {
        Point logical = ToLogical(point);
        return ActiveButtons.LastOrDefault(x => x.Visible && x.Bounds.Contains(logical));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        PixelButton? hit = ButtonAt(e.Location);
        bool changed = false;
        foreach (PixelButton button in _buttons.Concat(_modalButtons))
        {
            bool hovered = button == hit && button.Enabled;
            if (button.IsHovered == hovered) continue;
            button.IsHovered = hovered;
            changed = true;
        }
        if (changed) Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        foreach (PixelButton button in _buttons.Concat(_modalButtons)) button.IsHovered = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        _pressed = ButtonAt(e.Location);
        if (_pressed is { Enabled: true }) _pressed.IsPressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        PixelButton? pressed = _pressed;
        _pressed = null;
        if (pressed is null) return;
        pressed.IsPressed = false;
        Invalidate();
        // OnClick ekranı değiştirebilir; bu satırdan sonra bu ekrana dokunulmaz.
        if (pressed.Enabled && ButtonAt(e.Location) == pressed) pressed.OnClick();
    }
}
