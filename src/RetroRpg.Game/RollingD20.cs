using System.Numerics;

namespace RetroRpg.Game;

// Her üçgen bir sayı taşır. Son yönelim, oyun mantığının attığı yüzü kameraya çevirir.
internal static class RollingD20
{
    private const float Phi = 1.618034f;
    private static readonly Vector3[] Vertices =
    [
        new(-1,Phi,0),new(1,Phi,0),new(-1,-Phi,0),new(1,-Phi,0),
        new(0,-1,Phi),new(0,1,Phi),new(0,-1,-Phi),new(0,1,-Phi),
        new(Phi,0,-1),new(Phi,0,1),new(-Phi,0,-1),new(-Phi,0,1)
    ];
    private static readonly (int A,int B,int C)[] Faces =
    [
        (0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),(1,5,9),(5,11,4),(11,10,2),(10,7,6),(7,1,8),
        (3,9,4),(3,4,2),(3,2,6),(3,6,8),(3,8,9),(4,9,5),(2,4,11),(6,2,10),(8,6,7),(9,8,1)
    ];
    private static readonly string[] Digits = [
        "111101101101111","010110010010111","111001111100111","111001111001111","101101111001001",
        "111100111001111","111100111101111","111001010010010","111101111101111","111101111001111"];

    public static bool Landed(int ticks) => ticks <= 6;

    // Dönen zarın kameraya en yakın yüzünü döndürür. Çizim sırasında yeni zar atılmaz.
    public static int Draw(Graphics g, Rectangle box, int value, int ticks, int duration = 24)
    {
        var face = Faces[value - 1];
        Vector3 axisX = Vector3.Normalize(Vertices[face.B] - Vertices[face.A]);
        Vector3 normal = Vector3.Normalize(Vertices[face.A] + Vertices[face.B] + Vertices[face.C]);
        Vector3 axisY = Vector3.Cross(normal, axisX);
        float t = Math.Clamp((ticks - 6f) / (duration - 6), 0, 1);
        float spin = t * t;
        var rotation = Matrix4x4.CreateFromYawPitchRoll(8.9f * spin, 6.2f * spin, 4.2f * spin);
        Vector3 Turn(Vector3 p) => Vector3.Transform(new Vector3(Vector3.Dot(p,axisX),Vector3.Dot(p,axisY),Vector3.Dot(p,normal)),rotation);
        float radius = Math.Min(box.Width,box.Height) * .40f;
        float cx = box.X + box.Width*.5f - t * radius*.33f;
        float cy = box.Y + box.Height*.53f - Math.Abs(MathF.Sin(t * 8.7f))*t*radius*.39f;
        PointF Project(Vector3 p) => new(cx+p.X*radius/1.902f,cy-p.Y*radius/1.902f);
        using var shadow = new SolidBrush(Color.FromArgb(130,12,8,18));
        g.FillEllipse(shadow,cx-radius*.73f,box.Y+box.Height*.85f,radius*1.46f,radius*.22f);
        using var edge = new Pen(Color.FromArgb(213,161,80),Math.Max(1, radius*.025f));
        var visible = 0;
        float closest = -1;
        for (int i=0; i<Faces.Length; i++)
        {
            var (ia,ib,ic) = Faces[i];
            Vector3 a=Turn(Vertices[ia]), b=Turn(Vertices[ib]), c=Turn(Vertices[ic]);
            Vector3 center=(a+b+c)/3, n=Vector3.Normalize(center);
            if (n.Z <= 0) continue;
            if (n.Z > closest) { closest=n.Z; visible=i+1; }
            int light=(int)(38*n.Z + 18*n.Y - 13*n.X);
            using var fill = new SolidBrush(Color.FromArgb(71+light,35+light,89+light));
            PointF[] triangle=[Project(a),Project(b),Project(c)];
            g.FillPolygon(fill,triangle);
            g.DrawPolygon(edge,triangle);
            if (n.Z < .34f) continue;
            Vector3 right=Vector3.Normalize(b-a)*.088f;
            Vector3 down=-Vector3.Normalize(Vector3.Cross(n,b-a))*.105f;
            string number=(i+1).ToString();
            Vector3 origin=center-right*((number.Length*4-1)*.5f)-down*2.5f;
            for (int digit=0; digit<number.Length; digit++)
            for (int dot=0; dot<15; dot++)
            {
                if (Digits[number[digit]-'0'][dot]=='0') continue;
                Vector3 p=origin+right*(digit*4+dot%3)+down*(dot/3);
                g.FillPolygon(Brushes.Ivory,new PointF[]{Project(p),Project(p+right),Project(p+right+down),Project(p+down)});
            }
        }
        return visible;
    }
}
