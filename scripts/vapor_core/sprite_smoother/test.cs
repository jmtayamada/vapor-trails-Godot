using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

// TEXTURES THIS USES HAVE TO BE READABLE IN IMPORT SETTINGS
public partial class SpriteSmoother2 : Node
{
    const float scaleFactor = 4f;

    Dictionary<string, Image> upscaledImages = new Dictionary<string, Image>();
    Dictionary<string, Texture2D> upscaledTextures = new Dictionary<string, Texture2D>();
    HashSet<string> queuedImages = new HashSet<string>();

    List<SmoothSpriteChild> children = new List<SmoothSpriteChild>();

    void Start()
    {
        List<Sprite2D> SpriteChildren = new List<Sprite2D>();
        for (int i = 0; i < GetParent().GetChildCount(); i++)
        {
            SpriteChildren.Add(GetParent().GetChild<Sprite2D>(i));
        }

        foreach (Sprite2D s in SpriteChildren)
        {
            if (s.IsClass("NoSmoothSprite"))
            {
                continue;
            }
            SmoothSpriteChild newSmoothSpriteChild = new SmoothSpriteChild();
            newSmoothSpriteChild.Initialize(this);
            s.ReplaceBy(newSmoothSpriteChild, true);
            newSmoothSpriteChild.Name = s.Name;
            s.Free();
            children.Add(newSmoothSpriteChild);
        }
    }

    Task<AsyncImage> UpscaleTexture(AsyncImage input)
    {
        return Scale4x(input);
    }

    async Task<AsyncImage> Scale4x(AsyncImage input)
    {
        AsyncImage t = await Task.Run(() => Scale2x(Scale2x(input)));
        return t;
    }

    AsyncImage Scale2x(AsyncImage input)
    {
        AsyncImage output = new AsyncImage(new Vector2I(input.width * 2, input.height * 2));
        for (int x = 0; x < input.width; x++)
        {
            for (int y = 0; y < input.height; y++)
            {
                /*
					A B C
					D E F
					G H I

					E0 E1
					E2 E3
				*/
                Color b = input.GetPixel(x, y - 1);
                Color h = input.GetPixel(x, y + 1);
                Color d = input.GetPixel(x - 1, y);
                Color f = input.GetPixel(x + 1, y);

                Color e = input.GetPixel(x, y);

                if (!SameColor(b, h) && !SameColor(d, f))
                {
                    output.SetPixel(2 * x, 2 * y, SameColor(d, b) ? d : e);
                    output.SetPixel(2 * x + 1, 2 * y, SameColor(b, f) ? f : e);
                    output.SetPixel(2 * x, 2 * y + 1, SameColor(d, h) ? d : e);
                    output.SetPixel(2 * x + 1, 2 * y + 1, SameColor(h, f) ? f : e);
                }
                else
                {
                    output.SetPixel(2 * x, 2 * y, e);
                    output.SetPixel(2 * x + 1, 2 * y, e);
                    output.SetPixel(2 * x, 2 * y + 1, e);
                    output.SetPixel(2 * x + 1, 2 * y + 1, e);
                }
            }
        }
        return output;
    }

    bool SameColor(Color a, Color b)
    {
        return
            Mathf.IsEqualApprox(a.R, b.R)
            && Mathf.IsEqualApprox(a.G, b.G)
            && Mathf.IsEqualApprox(a.B, b.B)
            && Mathf.IsEqualApprox(a.A, b.A);
    }

    public Texture2D GetUpscaledSprite(Texture2D s, SmoothSpriteChild caller)
    {
        if (upscaledTextures.ContainsKey(s.ResourceName))
        {
            return upscaledTextures[s.ResourceName];
        }

        string imageName = s.GetImage().ResourceName;
        if (!upscaledImages.ContainsKey(imageName))
        {
            if (!queuedImages.Contains(imageName))
            {
                queuedImages.Add(imageName);
                AddUpscaledTexture(
                    new AsyncImage(s.texture.GetPixels(),
                    new Vector2I(s.texture.width, s.texture.height)),
                    textureName
                );
            }
            return s;
        }
        else
        {
            Sprite upscaled = ExtractSprite(s, upscaledTextures[textureName]);
            upscaledSprites.Add(s.name, upscaled);
            return upscaled;
        }
    }

    async void AddUpscaledTexture(AsyncImage texture, string name)
    {
        AsyncImage upscaledAsync = await UpscaleTexture(texture);
        Texture2D upscaled = new Texture2D(upscaledAsync.width, upscaledAsync.height);
        upscaled.filterMode = FilterMode.Point;
        upscaled.mipMapBias = -10;
        upscaled.SetPixels(upscaledAsync.GetPixels());
        upscaled.Apply(true, false);
        upscaled.name = name;
        upscaledTextures[name] = upscaled;
        queuedTextures.Remove(name);

        // abort the loop if the editor exits play mode
        if (!Application.isPlaying) return;

        foreach (SmoothSpriteChild c in children)
        {
            c.ForceUpscaledSprite();
        }
    }

    Sprite ExtractSprite(Sprite original, Texture2D upscaledAtlas)
    {
        Rect spriteRect = original.rect;
        spriteRect.position *= scaleFactor;
        spriteRect.size *= scaleFactor;

        Sprite upscaled = Sprite.Create(
            upscaledAtlas,
            spriteRect,
            original.pivot * scaleFactor / spriteRect.size,
            original.pixelsPerUnit * scaleFactor,
            1,
            SpriteMeshType.FullRect,
            original.border,
            false
        );

        upscaled.name = original.name;

        return upscaled;
    }
}
