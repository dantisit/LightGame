#!/usr/bin/env python3
"""
Generate a white cube texture with alpha gradient at edges.
Proportions: 11x0.3 (width x height ratio)
"""

from PIL import Image
import numpy as np

def generate_white_cube_with_alpha(width=1100, height=100, edge_fade=20):
    """
    Generate a solid white rectangle without alpha channel.

    Args:
        width: Width in pixels (default 1100 for 11:1 ratio with height 100)
        height: Height in pixels (default 100 for 11:0.3 ratio, scaled to 100)
        edge_fade: Not used, kept for compatibility
    """
    # Create RGB image (no alpha)
    img = np.zeros((height, width, 3), dtype=np.uint8)

    # Set RGB to white (255, 255, 255) - fully solid white everywhere
    img[:, :, 0] = 255  # R
    img[:, :, 1] = 255  # G
    img[:, :, 2] = 255  # B

    return Image.fromarray(img, 'RGB')

if __name__ == "__main__":
    # Generate texture with 11:0.3 proportion (scaled to reasonable pixel size)
    # Using 1100x100 pixels (11:1 ratio, where 0.3 is scaled to 1 for easier calculation)
    texture = generate_white_cube_with_alpha(width=1100, height=100, edge_fade=30)

    # Save to Assets folder
    output_path = "Assets/Light and controller/Materials/WhiteCubeTexture.png"
    texture.save(output_path)
    print(f"Texture generated and saved to: {output_path}")
    print(f"Dimensions: {texture.width}x{texture.height} pixels")
    print(f"Ratio: {texture.width/texture.height:.1f}:1 (approximately 11:0.3 when scaled)")
