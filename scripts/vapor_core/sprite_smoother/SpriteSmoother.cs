using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

// TEXTURES THIS USES HAVE TO BE READABLE IN IMPORT SETTINGS
public partial class SpriteSmoother : Node
{
	const float scaleFactor = 4f;

	Dictionary<string, Texture2D> upscaledTextures = new Dictionary<string, Texture2D>();
	HashSet<string> queuedTextures = new HashSet<string>();

	List<SmoothSpriteChild> children = new List<SmoothSpriteChild>();

	void Start()
	{
		List<NoSmoothSprite> SpriteChildren = new List<NoSmoothSprite>();
		for (int i = 0; i < GetParent().GetChildCount(); i++)
		{
			SpriteChildren.Add(GetParent().GetChild<NoSmoothSprite>(i));
		}

		foreach (NoSmoothSprite s in SpriteChildren)
		{
			SmoothSpriteChild newSmoothSpriteChild = new SmoothSpriteChild();
			newSmoothSpriteChild.Initialize(this);
			s.ReplaceBy(newSmoothSpriteChild, true);
			newSmoothSpriteChild.Name = s.Name;
			s.Free();
			children.Add(newSmoothSpriteChild);
		}
	}

	Task<AsyncTexture> UpscaleTexture(AsyncTexture input)
	{
		return Scale4x(input);
	}

	async Task<AsyncTexture> Scale4x(AsyncTexture input)
	{
		AsyncTexture t = await Task.Run(() => Scale2x(Scale2x(input)));
		return t;
	}

	AsyncTexture Scale2x(AsyncTexture input)
	{
		AsyncTexture output = new AsyncTexture(new Vector2I(input.width * 2, input.height * 2));
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

	public Texture2D GetUpscaledTexture(Texture2D s, SmoothSpriteChild caller)
	{
		if (upscaledTextures.ContainsKey(s.ResourceName))
		{
			return upscaledTextures[s.ResourceName];
		}

		string textureName = s.ResourceName;

		if (!queuedTextures.Contains(textureName))
		{
			queuedTextures.Add(textureName);
			AddUpscaledTexture(
				new AsyncTexture(s.texture.GetPixels(),
				new Vector2I(s.GetWidth(), s.GetHeight())),
				textureName
			);
		}
		return s;

	}

	async void AddUpscaledTexture(AsyncTexture texture, string name)
	{
		AsyncTexture upscaledAsync = await UpscaleTexture(texture);
		Texture2D upscaled = new Texture2D(upscaledAsync.width, upscaledAsync.height);
		upscaled.filterMode = FilterMode.Point;
		upscaled.mipMapBias = -10;
		upscaled.SetPixels(upscaledAsync.GetPixels());
		upscaled.Apply(true, false);
		upscaled.ResourceName = name;
		upscaledTextures[name] = upscaled;
		queuedTextures.Remove(name);

		// abort the loop if the editor exits play mode
		if (!Application.isPlaying) return;

		foreach (SmoothSpriteChild c in children)
		{
			c.ForceUpscaledSprite();
		}
	}
}
