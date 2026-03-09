#!/usr/bin/env python3
"""
创建 Windows ICO 图标文件
ICO 文件格式: https://en.wikipedia.org/wiki/ICO_(file_format)

使用方法:
    python3 create_ico.py

这个脚本会读取 Assets.xcassets 中的 PNG 图标，生成 AppIcon.ico
"""

import struct
import os
import zlib

def read_png_file(path):
    """读取 PNG 文件并返回原始数据"""
    with open(path, 'rb') as f:
        return f.read()

def get_png_dimensions(png_data):
    """从 PNG 数据中提取宽度和高度"""
    # PNG 签名: 8 bytes
    # IHDR chunk: 4 bytes length + 4 bytes "IHDR" + 4 bytes width + 4 bytes height + ...
    if png_data[:8] != b'\x89PNG\r\n\x1a\n':
        raise ValueError("Not a valid PNG file")
    
    width = struct.unpack('>I', png_data[16:20])[0]
    height = struct.unpack('>I', png_data[20:24])[0]
    return width, height

def create_ico(png_files, output_path):
    """
    创建 ICO 文件
    
    ICO 文件格式:
    - ICONDIR header (6 bytes)
    - ICONDIRENTRY array (16 bytes each)
    - Image data (PNG format)
    """
    
    images = []
    for png_path in png_files:
        if os.path.exists(png_path):
            data = read_png_file(png_path)
            w, h = get_png_dimensions(data)
            # ICO 格式中，256 用 0 表示
            images.append({
                'width': 0 if w >= 256 else w,
                'height': 0 if h >= 256 else h,
                'data': data,
                'size': len(data)
            })
    
    if not images:
        print("No valid PNG files found!")
        return False
    
    # ICO Header (ICONDIR)
    # 2 bytes: Reserved (0)
    # 2 bytes: Type (1 = ICO)
    # 2 bytes: Number of images
    header = struct.pack('<HHH', 0, 1, len(images))
    
    # Calculate offsets
    # Header: 6 bytes
    # Each entry: 16 bytes
    data_offset = 6 + len(images) * 16
    
    entries = []
    image_data = []
    current_offset = data_offset
    
    for img in images:
        # ICONDIRENTRY (16 bytes)
        # 1 byte: Width (0 = 256)
        # 1 byte: Height (0 = 256)
        # 1 byte: Color count (0 = no palette)
        # 1 byte: Reserved (0)
        # 2 bytes: Color planes (1 for ICO)
        # 2 bytes: Bits per pixel (32 for RGBA)
        # 4 bytes: Size of image data
        # 4 bytes: Offset to image data
        entry = struct.pack('<BBBBHHII',
            img['width'],
            img['height'],
            0,  # color count
            0,  # reserved
            1,  # color planes
            32, # bits per pixel
            img['size'],
            current_offset
        )
        entries.append(entry)
        image_data.append(img['data'])
        current_offset += img['size']
    
    # Write ICO file
    with open(output_path, 'wb') as f:
        f.write(header)
        for entry in entries:
            f.write(entry)
        for data in image_data:
            f.write(data)
    
    print(f"Created {output_path} with {len(images)} image(s)")
    return True

def main():
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.dirname(script_dir)  # Rectangle.Windows
    repo_root = os.path.dirname(project_root)   # Rectangle.For.Windows
    
    # macOS Rectangle 项目的图标路径
    macos_icons_dir = os.path.join(repo_root, 'Rectangle', 'Assets.xcassets', 'AppIcon.appiconset')
    
    # Windows 项目的 Assets 路径
    windows_assets_dir = os.path.join(project_root, 'src', 'Rectangle.Windows', 'Assets')
    
    print(f"Looking for icons in: {macos_icons_dir}")
    print(f"Output directory: {windows_assets_dir}")
    
    # ICO 文件需要的尺寸 (Windows 标准尺寸)
    # 16x16, 32x32, 48x48, 64x64, 128x128, 256x256
    icon_mappings = [
        ('mac016pts1x.png', 16),   # 16x16
        ('mac016pts2x.png', 32),   # 32x32 (实际是 16pts@2x)
        ('mac032pts2x.png', 64),   # 64x64 (32pts@2x)
        ('mac128pts1x.png', 128),  # 128x128
        ('mac256pts1x.png', 256),  # 256x256
    ]
    
    png_files = []
    for filename, expected_size in icon_mappings:
        path = os.path.join(macos_icons_dir, filename)
        if os.path.exists(path):
            png_files.append(path)
            print(f"Found: {filename} ({expected_size}x{expected_size})")
        else:
            print(f"Missing: {filename}")
    
    if png_files:
        output_path = os.path.join(windows_assets_dir, 'AppIcon.ico')
        create_ico(png_files, output_path)
        print(f"\nICO file created at: {output_path}")
    else:
        print("No icon files found!")

if __name__ == '__main__':
    main()
