using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

// TEXTURES THIS USES HAVE TO BE READABLE IN IMPORT SETTINGS
public partial class SpriteSmoother : Node
{
	const float scaleFactor = 4f;

	Dictionary<string, Image> upscaledImages = new Dictionary<string, Image>();
	Dictionary<string, Texture2D> upscaledTextures = new Dictionary<string, Texture2D>();
	HashSet<string> queuedImages = new HashSet<string>();

	List<SmoothSpriteChild> children = new List<SmoothSpriteChild>();

	public override void _Ready()
	{
		foreach (Sprite2D s in GetParent().FindChildren("*", "Sprite2D"))
		{
			if (s.IsClass("NoSmoothSprite") || NodeHelper.getComponent<NoSmoothSprite>(s) != default(NoSmoothSprite))
			{
				continue;
			}
			SmoothSpriteChild newSmoothSpriteChild = new SmoothSpriteChild();
			s.AddChild(newSmoothSpriteChild);
			newSmoothSpriteChild.Initialize(this);
			newSmoothSpriteChild.Name = s.Name;
			children.Add(newSmoothSpriteChild);
		}
		GD.Print(children.Count);
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

	public Texture2D GetUpscaledTexture(Texture2D s, SmoothSpriteChild caller)
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

				Color[] pixels = new Color[s.GetImage().GetWidth() * s.GetImage().GetHeight()];
				for (int x = 0; x < s.GetImage().GetWidth(); x++)
				{
					for (int y = 0; y < s.GetImage().GetHeight(); y++)
					{
						pixels[x * y] = s.GetImage().GetPixel(x, y);
					}
				}

				AddUpscaledTexture(
					new AsyncImage(pixels,
					new Vector2I(s.GetImage().GetWidth(), s.GetImage().GetHeight())),
					imageName
				);
			}
			return s;
		}
		else
		{
			Texture2D upscaled = ExtractTexture(s, upscaledImages[imageName]);
			upscaledTextures.Add(s.ResourceName, upscaled);
			return upscaled;
		}
	}

	async void AddUpscaledTexture(AsyncImage texture, string name)
	{
		AsyncImage upscaledAsync = await UpscaleTexture(texture);
		Image upscaled = Image.CreateEmpty(upscaledAsync.width, upscaledAsync.height, true, Image.Format.Rgba8);
		for (int x = 0; x < upscaledAsync.width; x++)
		{
			for (int y = 0; y < upscaledAsync.height; y++)
			{
				upscaled.SetPixel(x, y, upscaledAsync.GetPixel(x, y));
			}
		}
		upscaled.ResourceName = name;
		upscaledImages[name] = upscaled;
		queuedImages.Remove(name);

		// // abort the loop if the editor exits play mode
		// if (!Application.isPlaying) return;

		foreach (SmoothSpriteChild c in children)
		{
			c.ForceUpscaledSprite();
		}
	}

	Texture2D ExtractTexture(Texture2D original, Image upscaledAtlas)
	{
		Texture2D upscaled = ImageTexture.CreateFromImage(upscaledAtlas);

		upscaled.ResourceName = original.ResourceName;

		return upscaled;
	}
}
