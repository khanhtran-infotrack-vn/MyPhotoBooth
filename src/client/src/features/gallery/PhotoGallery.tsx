import { useQuery } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import api from '../../lib/api';
import PhotoUpload from '../upload/PhotoUpload';
import { useAuthStore } from '../../stores/authStore';

interface Photo {
  id: string;
  originalFileName: string;
  capturedAt: string | null;
  uploadedAt: string;
  thumbnailPath: string;
}

function Lightbox({ photo, onClose }: { photo: Photo; onClose: () => void }) {
  const [photoUrl, setPhotoUrl] = useState<string>('');

  useEffect(() => {
    const loadPhoto = async () => {
      try {
        const response = await api.get(`/photos/${photo.id}/file`, {
          responseType: 'blob',
        });
        const url = URL.createObjectURL(response.data);
        setPhotoUrl(url);

        return () => URL.revokeObjectURL(url);
      } catch (error) {
        console.error('Failed to load photo:', error);
      }
    };

    loadPhoto();
  }, [photo.id]);

  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.95)',
        backdropFilter: 'blur(10px)',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        padding: '20px',
        zIndex: 1000,
        animation: 'fadeIn 0.2s ease-in-out',
      }}
      onClick={onClose}
    >
      {/* Close button */}
      <button
        onClick={onClose}
        style={{
          position: 'absolute',
          top: '20px',
          right: '20px',
          background: 'rgba(255, 255, 255, 0.1)',
          border: 'none',
          color: 'white',
          width: '48px',
          height: '48px',
          borderRadius: '50%',
          fontSize: '24px',
          cursor: 'pointer',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          transition: 'background 0.2s',
          backdropFilter: 'blur(10px)',
        }}
        onMouseOver={(e) => {
          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.2)';
        }}
        onMouseOut={(e) => {
          e.currentTarget.style.background = 'rgba(255, 255, 255, 0.1)';
        }}
      >
        ✕
      </button>

      {/* Photo info */}
      <div style={{
        position: 'absolute',
        bottom: '20px',
        left: '50%',
        transform: 'translateX(-50%)',
        background: 'rgba(255, 255, 255, 0.1)',
        backdropFilter: 'blur(10px)',
        padding: '12px 24px',
        borderRadius: '12px',
        color: 'white',
        fontSize: '14px',
        maxWidth: '90%',
        textAlign: 'center',
      }}>
        <p style={{ margin: 0, fontWeight: '500' }}>{photo.originalFileName}</p>
        {photo.capturedAt && (
          <p style={{ margin: '4px 0 0 0', fontSize: '12px', opacity: 0.8 }}>
            {new Date(photo.capturedAt).toLocaleDateString()}
          </p>
        )}
      </div>

      {/* Photo */}
      {photoUrl ? (
        <img
          src={photoUrl}
          alt={photo.originalFileName}
          style={{
            maxWidth: '90%',
            maxHeight: '90%',
            objectFit: 'contain',
            borderRadius: '12px',
            boxShadow: '0 20px 60px rgba(0, 0, 0, 0.5)',
          }}
          onClick={(e) => e.stopPropagation()}
        />
      ) : (
        <div style={{
          color: 'white',
          fontSize: '18px',
          display: 'flex',
          alignItems: 'center',
          gap: '12px',
        }}>
          <div style={{
            width: '24px',
            height: '24px',
            border: '3px solid rgba(255, 255, 255, 0.3)',
            borderTopColor: 'white',
            borderRadius: '50%',
            animation: 'spin 1s linear infinite',
          }} />
          Loading...
        </div>
      )}

      <style>
        {`
          @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
          }
          @keyframes spin {
            to { transform: rotate(360deg); }
          }
        `}
      </style>
    </div>
  );
}

export default function PhotoGallery() {
  const [selectedPhoto, setSelectedPhoto] = useState<Photo | null>(null);
  const [thumbnailUrls, setThumbnailUrls] = useState<Record<string, string>>({});
  const logout = useAuthStore((state) => state.logout);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['photos'],
    queryFn: async () => {
      const response = await api.get('/photos');
      return response.data.items as Photo[];
    },
  });

  // Fetch thumbnails with authentication
  useEffect(() => {
    if (!data) return;

    const fetchThumbnails = async () => {
      const urls: Record<string, string> = {};

      for (const photo of data) {
        try {
          const response = await api.get(`/photos/${photo.id}/thumbnail`, {
            responseType: 'blob',
          });
          urls[photo.id] = URL.createObjectURL(response.data);
        } catch (error) {
          console.error(`Failed to load thumbnail for ${photo.id}:`, error);
        }
      }

      setThumbnailUrls(urls);
    };

    fetchThumbnails();

    // Cleanup blob URLs on unmount
    return () => {
      Object.values(thumbnailUrls).forEach(url => URL.revokeObjectURL(url));
    };
  }, [data]);

  const getThumbnailUrl = (photoId: string) => {
    return thumbnailUrls[photoId] || '';
  };

  const handleLogout = async () => {
    await logout();
    window.location.href = '/login';
  };

  return (
    <div style={{
      minHeight: '100vh',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      padding: '20px'
    }}>
      {/* Header */}
      <div style={{
        maxWidth: '1400px',
        margin: '0 auto',
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginBottom: '40px',
        padding: '20px',
        background: 'rgba(255, 255, 255, 0.95)',
        borderRadius: '16px',
        boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
      }}>
        <h1 style={{
          margin: 0,
          fontSize: '32px',
          fontWeight: '700',
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          WebkitBackgroundClip: 'text',
          WebkitTextFillColor: 'transparent',
        }}>
          📸 My Photo Booth
        </h1>
        <button
          onClick={handleLogout}
          style={{
            padding: '12px 24px',
            background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
            color: 'white',
            border: 'none',
            borderRadius: '12px',
            fontWeight: '600',
            fontSize: '14px',
            cursor: 'pointer',
            transition: 'transform 0.2s, box-shadow 0.2s',
            boxShadow: '0 4px 12px rgba(102, 126, 234, 0.4)',
          }}
          onMouseOver={(e) => {
            e.currentTarget.style.transform = 'translateY(-2px)';
            e.currentTarget.style.boxShadow = '0 6px 20px rgba(102, 126, 234, 0.6)';
          }}
          onMouseOut={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 4px 12px rgba(102, 126, 234, 0.4)';
          }}
        >
          Logout
        </button>
      </div>

      {/* Main Content */}
      <div style={{ maxWidth: '1400px', margin: '0 auto' }}>
        <PhotoUpload onUploadComplete={() => refetch()} />

        {/* Gallery Section */}
        <div style={{
          marginTop: '40px',
          background: 'rgba(255, 255, 255, 0.95)',
          borderRadius: '16px',
          padding: '30px',
          boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
        }}>
          <div style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '30px',
          }}>
            <h2 style={{
              margin: 0,
              fontSize: '24px',
              fontWeight: '600',
              color: '#1a202c',
            }}>
              Your Photos
            </h2>
            <span style={{
              padding: '8px 16px',
              background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
              color: 'white',
              borderRadius: '20px',
              fontSize: '14px',
              fontWeight: '600',
            }}>
              {data?.length || 0} photos
            </span>
          </div>

          {isLoading && (
            <div style={{ textAlign: 'center', padding: '40px', color: '#718096' }}>
              <p style={{ fontSize: '16px' }}>Loading your memories...</p>
            </div>
          )}

          {data && data.length === 0 && !isLoading && (
            <div style={{
              textAlign: 'center',
              padding: '60px 20px',
              color: '#718096',
            }}>
              <p style={{ fontSize: '18px', marginBottom: '10px' }}>📷 No photos yet</p>
              <p style={{ fontSize: '14px', color: '#a0aec0' }}>Upload your first photo to get started!</p>
            </div>
          )}

          <div style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))',
            gap: '20px',
          }}>
            {data?.map((photo) => (
              <div
                key={photo.id}
                style={{
                  cursor: 'pointer',
                  borderRadius: '16px',
                  overflow: 'hidden',
                  aspectRatio: '1',
                  background: '#f7fafc',
                  boxShadow: '0 4px 12px rgba(0, 0, 0, 0.08)',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                }}
                onClick={() => setSelectedPhoto(photo)}
                onMouseOver={(e) => {
                  e.currentTarget.style.transform = 'translateY(-4px) scale(1.02)';
                  e.currentTarget.style.boxShadow = '0 12px 24px rgba(0, 0, 0, 0.15)';
                }}
                onMouseOut={(e) => {
                  e.currentTarget.style.transform = 'translateY(0) scale(1)';
                  e.currentTarget.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.08)';
                }}
              >
                <img
                  src={getThumbnailUrl(photo.id)}
                  alt={photo.originalFileName}
                  style={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover',
                  }}
                />
              </div>
            ))}
          </div>
        </div>
      </div>

      {selectedPhoto && <Lightbox photo={selectedPhoto} onClose={() => setSelectedPhoto(null)} />}
    </div>
  );
}
