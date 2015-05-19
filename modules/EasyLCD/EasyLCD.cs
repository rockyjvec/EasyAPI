public class EasyLCD
{
    public char[] buffer;
    IMyTextPanel screen;
    EasyBlock block;

    public int width;
    public int height;

    public int xDisplays = 0;
    public int yDisplays = 0;

    private int wPanel = 36;
    private int hPanel = 18;

    Single fontSize;

    public EasyLCD(EasyBlocks block, double scale = 1.0)
    {
        this.block = block.GetBlock(0);
        if(this.block.Type() == "Wide LCD panel") this.wPanel = 72;

        this.screen = (IMyTextPanel)(block.GetBlock(0).Block);
        this.fontSize = block.GetProperty<Single>("FontSize");

        this.width = (int)((double)this.wPanel / this.fontSize);
        this.height = (int)((double)this.hPanel / this.fontSize);
        this.buffer = new char[this.width * this.height];
        this.clear();
        this.update();
    }

    public void SetText(String text, bool append = false)
    {
        this.screen.WritePublicText(text, append);
    }

    public void plot(EasyBlocks blocks, double x, double y, double scale = 1.0, char brush = 'o', bool showBounds = true, char boundingBrush = '?')
    {
        VRageMath.Vector3D max = new Vector3D(this.screen.CubeGrid.Max);
        VRageMath.Vector3D min = new Vector3D(this.screen.CubeGrid.Min);
        VRageMath.Vector3D size = new Vector3D(max - min);

        int width = (int)size.GetDim(0);
        int height = (int)size.GetDim(1);
        int depth = (int)size.GetDim(2);

        int minX = (int)min.GetDim(0);
        int minY = (int)min.GetDim(1);
        int minZ = (int)min.GetDim(2);

        int maxX = (int)max.GetDim(0);
        int maxY = (int)max.GetDim(1);
        int maxZ = (int)max.GetDim(2);

        double s = (double)depth + 0.01;
        if(width > depth)
        {
            s = (double)width + 0.01;
        }

        if(showBounds)
        {
            box(x + -(((0 - (width / 2.0)) / s) * scale),
                y + -(((0 - (depth / 2.0)) / s) * scale),
                x + -(((maxX - minX - (width / 2.0)) / s) * scale),
                y + -(((maxZ - minZ - (depth / 2.0)) / s) * scale), boundingBrush);
        }

        for(int n = 0; n < blocks.Count(); n++)
        {
            var block = blocks.GetBlock(n);

            Vector3D pos = new Vector3D(block.Block.Position);

            pset(x + -((((double)(pos.GetDim(0) - minX - (width / 2.0)) / s)) * scale),
                 y + -((((double)(pos.GetDim(2) - minZ - (depth / 2.0)) / s)) * scale), brush);
        }
    }

    // draw a pixel to the buffer
    public void pset(double x, double y, char brush = 'o')
    {
        if(x >= 0 && x < 1 && y >= 0 && y < 1)
        {
            this.buffer[this.linear(x, y)] = brush;
        }
    }

    private void pset(int x, int y, char brush = '0')
    {
        if(x >= 0 && x < this.width && y >= 0 && y < this.height)
        {
            this.buffer[x + (y * this.width)] = brush;
        }
    }

    public void text(double x, double y, String text)
    {
        int xx = (int)(x * (this.width - 1));
        int yy = (int)(y * (this.height - 1));

        for(int xs = 0; xs < text.Length; xs++)
        {
            pset(xx + xs, yy, text[xs]);
        }
    }

    // clear the buffer
    public void clear(char brush = ' ')
    {
        for(int n = 0; n < this.buffer.Length; n++)
        {
            this.buffer[n] = brush;
        }
    }

    // Transfer buffer contents to the lcd
    public void update()
    {
        String s = "";
        String space = "";

        //this.screen.WritePublicText(clearBuf);

        for(int y = 0; y < this.height; y++)
        {
            space = "";
            for(int x = 0; x < this.width; x++)
            {
                if(buffer[x + (y * this.width)] == ' ')
                {
                    space += "  ";
                }
                else
                {
                    s += space + buffer[x + (y * this.width)];
                    space = "";
                }
            }
            s += "\n";
        }

        this.screen.WritePublicText(s);
    }

    private int linear(double x, double y)
    {
        int xx = (int)(x * (this.width - 1));
        int yy = (int)(y * (this.height - 1));
        return xx + yy * this.width;
    }

    public void line(double xx0, double yy0, double xx1, double yy1, char brush = 'o')
    {
        int x0 = (int)Math.Floor(xx0 * (this.width));
        int y0 = (int)Math.Floor(yy0 * (this.height));
        int x1 = (int)Math.Floor(xx1 * (this.width));
        int y1 = (int)Math.Floor(yy1 * (this.height));

        bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
        if (steep)
        {
            int tmp = x0;
            x0 = y0;
            y0 = tmp;
            tmp = x1;
            x1 = y1;
            y1 = tmp;
        }

        if (x0 > x1)
        {
            int tmp = x0;
            x0 = x1;
            x1 = tmp;
            tmp = y0;
            y0 = y1;
            y1 = tmp;
        }

        int dX = (x1 - x0), dY = Math.Abs(y1 - y0), err = (dX / 2), ystep = (y0 < y1 ? 1 : -1), y = y0;

        for (int x = x0; x <= x1; ++x)
        {
            if(steep)
                pset(y, x, brush);
            else
                pset(x, y, brush);
            err = err - dY;
            if (err < 0) { y += ystep;  err += dX; }
        }
    }

    public void box(double x0, double y0, double x1, double y1, char brush = 'o')
    {
        line(x0, y0, x1, y0, brush);
        line(x1, y0, x1, y1, brush);
        line(x1, y1, x0, y1, brush);
        line(x0, y1, x0, y0, brush);
    }
}
