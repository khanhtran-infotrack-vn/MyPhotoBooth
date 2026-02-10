# Create minimal valid JPEG
import struct

def create_jpeg(filename, width=800, height=600):
    # Minimal JPEG with blue color
    with open(filename, 'wb') as f:
        # JPEG SOI marker
        f.write(b'\xff\xd8')
        
        # APP0 marker (JFIF identifier)
        f.write(b'\xff\xe0')
        f.write(struct.pack('>H', 16))  # Length
        f.write(b'JFIF\x00')
        f.write(b'\x01\x01')  # Version
        f.write(b'\x00')      # Units (aspect ratio)
        f.write(struct.pack('>HH', 1, 1))  # Aspect ratio
        f.write(b'\x00\x00')  # Thumbnail dimensions
        
        # SOF0 marker (Start of Frame, baseline DCT)
        f.write(b'\xff\xc0')
        f.write(struct.pack('>H', 8 + 3 * 3))  # Length
        f.write(b'\x08')  # Precision
        f.write(struct.pack('>H', height))
        f.write(struct.pack('>H', width))
        f.write(b'\x03')  # Number of components
        for i in range(3):
            f.write(bytes([i + 1, 0x11, 0 if i == 0 else 1]))  # Component ID, sampling, QT
        
        # DQT marker (Define Quantization Table)
        for precision in [0, 1]:
            f.write(b'\xff\xdb')
            f.write(struct.pack('>H', 2 + 64 * (1 if precision == 0 else 2)))
            f.write(bytes([precision + 16]))
            # Standard luminance/chrominance tables
            table = [16,11,10,16,24,40,51,61,12,12,14,19,26,58,60,55,
                    14,13,16,24,40,57,69,56,14,17,22,29,51,87,80,62,
                    18,22,37,56,68,109,103,77,24,35,55,64,81,104,113,92,
                    49,64,78,87,103,121,120,101,72,92,95,98,112,100,103,99] if precision == 0 else \
                   [17,18,24,47,99,99,99,99,18,21,26,66,99,99,99,99,
                    24,26,56,99,99,99,99,99,47,66,99,99,99,99,99,99,
                    99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,
                    99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99]
            f.write(bytes(table[:64]))
        
        # DHT marker (Define Huffman Table) - simplified
        for i in range(4):
            f.write(b'\xff\xc4')
            f.write(struct.pack('>H', 2 + 16))
            f.write(bytes([i & 0x10, 0]))  # Table class and identifier
            f.write(bytes([1] * 16))  # 16 bytes of counts
            f.write(bytes([0] * 16))  # 16 symbols
        
        # SOS marker (Start of Scan)
        f.write(b'\xff\xda')
        f.write(struct.pack('>H', 6 + 2 * 3))
        f.write(b'\x03')  # Number of components
        for i in range(3):
            f.write(bytes([i + 1, i & 1]))  # Component selector, DC/AC table
        f.write(b'\x00\x3f\x00')  # Spectral selection, successive approximation
        
        # Minimal scan data (1 MCU, blue color)
        f.write(b'\x00\x01')  # 8x8 block with minimal data
        f.write(b'\xff\xd9')  # EOI marker

create_jpeg('/Users/trankhanh/Desktop/MyProjects/MyPhotoBooth/plans/e2e-testing-20260210/test-photo.jpg')
print("Created test-photo.jpg")
