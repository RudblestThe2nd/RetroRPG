namespace RetroRpg.Game;

// Değerler anlık CAN'dan hesaplanır. İyileşince izler de aynı karede azalır.
internal readonly record struct BattleWounds(int Stage, int Flash)
{
    public static BattleWounds FromHealth(int health, int maximum, int flash = 0)
    {
        int stage = health * 4L <= maximum ? 3 : health * 2L <= maximum ? 2 : health * 4L <= maximum * 3L ? 1 : 0;
        return new(stage, flash);
    }

    public void DrawMarks(Bitmap sprite, int top, int bottom)
    {
        if (Stage == 0 || bottom <= top) return;
        DrawCut(sprite, top, bottom, 44, 57, 1);
        if (Stage >= 2) DrawCut(sprite, top, bottom, 62, 42, -1);
        if (Stage >= 3) DrawCut(sprite, top, bottom, 32, 47, 1);
    }

    // Her çizik o satırdaki en geniş dolu parçaya oturur. Şeffaf kenar, kanat ve
    // silah boşlukları farklı olsa da iz piksel silüetinin dışına taşmaz.
    private static void DrawCut(Bitmap sprite, int top, int bottom, int heightPercent, int widthPercent, int slope)
    {
        int y = top + (bottom - top) * heightPercent / 100;
        int start = 0, length = 0, run = 0;
        for (int x = 0; x < sprite.Width; x++)
        {
            run = sprite.GetPixel(x, y).A == 0 ? 0 : run + 1;
            if (run > length) { length = run; start = x - run + 1; }
        }
        if (length < 3) return;
        int center = start + length * widthPercent / 100;
        int radius = Math.Clamp(length / 6, 2, 5);
        for (int i = -radius; i <= radius; i++)
        {
            int row = y + slope * (i + radius) / 2 - slope * (radius / 2);
            Mark(center + i, row, Color.FromArgb(96, 32, 32));
            Mark(center + i, row - 1, Color.FromArgb(208, 80, 64));
            if (Math.Abs(i) < radius / 2 + 1) Mark(center + i, row + 1, Color.FromArgb(112, 48, 48));
        }

        void Mark(int x, int row, Color color)
        {
            if (x >= 0 && x < sprite.Width && row >= 0 && row < sprite.Height && sprite.GetPixel(x, row).A != 0)
                sprite.SetPixel(x, row, color);
        }
    }
}
