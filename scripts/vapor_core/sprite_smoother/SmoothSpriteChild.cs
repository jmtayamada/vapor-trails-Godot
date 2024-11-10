using Godot;

public partial class SmoothSpriteChild : Node
{
	SpriteSmoother spriteSmoother;
	string spriteLastFrame;
	Texture2D texture;

	public void Initialize(SpriteSmoother s)
	{
		spriteSmoother = s;
		texture = GetParent<Sprite2D>().Texture;
		texture = spriteSmoother.GetUpscaledTexture(texture, this);
		spriteLastFrame = texture.ResourceName;
	}

	public override void _Process(double delta)
	{
		CallDeferred("LateUpdate", delta);
	}

	public void LateUpdate(double _delta)
	{
		// if the sprites differ, get the upscaled version from spritesmoother
		// sprites may differ every frame due to some animator keying, or they may not
		// so for performance reasons just check every frame, and optimize the check if needed later
		if (texture != null && texture.ResourceName != spriteLastFrame)
		{
			texture = spriteSmoother.GetUpscaledTexture(texture, this);
			spriteLastFrame = texture.ResourceName;
		}
	}

	public void ForceUpscaledSprite()
	{
		texture = spriteSmoother.GetUpscaledTexture(texture, this);
		spriteLastFrame = texture.ResourceName;
	}
}
