using Godot;

public partial class SmoothSpriteChild : Sprite2D
{
	SpriteSmoother spriteSmoother;
	string spriteLastFrame;

	public void Initialize(SpriteSmoother s)
	{
		ProcessPriority = 10;
		spriteSmoother = s;
		Texture = spriteSmoother.GetUpscaledTexture(Texture, this);
		spriteLastFrame = Texture.ResourceName;
	}

	public override void _Process(double _delta)
	{
		// if the sprites differ, get the upscaled version from spritesmoother
		// sprites may differ every frame due to some animator keying, or they may not
		// so for performance reasons just check every frame, and optimize the check if needed later
		if (Texture != null && Texture.ResourceName != spriteLastFrame)
		{
			Texture = spriteSmoother.GetUpscaledTexture(Texture, this);
			spriteLastFrame = Texture.ResourceName;
		}
	}

	public void ForceUpscaledSprite()
	{
		Texture = spriteSmoother.GetUpscaledTexture(Texture, this);
		spriteLastFrame = Texture.ResourceName;
	}
}
