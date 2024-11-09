using Godot;

public class AsyncImage
{
	public int width { get; private set; }
	public int height { get; private set; }
	Color[] pixels;

	public AsyncImage(Color[] pixels, Vector2I size)
	{
		this.pixels = pixels;
		width = size.X;
		height = size.Y;
	}

	public AsyncImage(Vector2I size)
	{
		this.pixels = new Color[size.X * size.Y];
		this.width = size.X;
		this.height = size.Y;
	}

	public Color GetPixel(int x, int y)
	{
		x = Mathf.Clamp(x, 0, width - 1);
		y = Mathf.Clamp(y, 0, height - 1);

		// origin is at the bottom left
		return pixels[y * width + x];
	}

	public void SetPixel(int x, int y, Color color)
	{
		if (x >= width) x = width - 1;
		else if (x < 0) x = 0;
		if (y >= height) y = height - 1;
		else if (y < 0) y = 0;

		pixels[y * width + x] = color;
	}

	// public Color[] GetPixels()
	// {
	// 	return pixels;
	// }
}
