import { useState, useRef } from 'react';
import api from '../../lib/api';

export default function PhotoUpload({ onUploadComplete }: { onUploadComplete?: () => void }) {
  const [uploading, setUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [dragActive, setDragActive] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFiles = async (files: FileList) => {
    if (!files || files.length === 0) return;

    setUploading(true);
    setError('');
    setSuccess('');
    setUploadProgress(0);

    try {
      const filesArray = Array.from(files);
      for (let i = 0; i < filesArray.length; i++) {
        const file = filesArray[i];
        const formData = new FormData();
        formData.append('file', file);

        await api.post('/photos', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });

        setUploadProgress(Math.round(((i + 1) / filesArray.length) * 100));
      }

      setSuccess(`✓ ${filesArray.length} photo(s) uploaded successfully!`);
      if (onUploadComplete) onUploadComplete();
      if (fileInputRef.current) fileInputRef.current.value = '';

      setTimeout(() => setSuccess(''), 3000);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Upload failed');
    } finally {
      setUploading(false);
      setUploadProgress(0);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      handleFiles(e.target.files);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFiles(e.dataTransfer.files);
    }
  };

  const handleClick = () => {
    fileInputRef.current?.click();
  };

  return (
    <div style={{
      background: 'rgba(255, 255, 255, 0.95)',
      borderRadius: '16px',
      padding: '30px',
      boxShadow: '0 8px 32px rgba(0, 0, 0, 0.1)',
    }}>
      {error && (
        <div style={{
          padding: '12px 20px',
          background: '#fee',
          color: '#c33',
          borderRadius: '12px',
          marginBottom: '20px',
          fontSize: '14px',
          fontWeight: '500',
        }}>
          ✕ {error}
        </div>
      )}

      {success && (
        <div style={{
          padding: '12px 20px',
          background: '#d4edda',
          color: '#155724',
          borderRadius: '12px',
          marginBottom: '20px',
          fontSize: '14px',
          fontWeight: '500',
        }}>
          {success}
        </div>
      )}

      <div
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
        onClick={handleClick}
        style={{
          border: dragActive ? '3px dashed #667eea' : '3px dashed #e2e8f0',
          borderRadius: '16px',
          padding: '60px 20px',
          textAlign: 'center',
          cursor: uploading ? 'not-allowed' : 'pointer',
          background: dragActive ? 'rgba(102, 126, 234, 0.05)' : 'transparent',
          transition: 'all 0.2s',
          position: 'relative',
        }}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          multiple
          onChange={handleFileChange}
          disabled={uploading}
          style={{ display: 'none' }}
        />

        {uploading ? (
          <div>
            <div style={{
              width: '60px',
              height: '60px',
              border: '4px solid #e2e8f0',
              borderTopColor: '#667eea',
              borderRadius: '50%',
              margin: '0 auto 20px',
              animation: 'spin 1s linear infinite',
            }} />
            <p style={{
              fontSize: '18px',
              fontWeight: '600',
              color: '#1a202c',
              margin: '0 0 10px 0',
            }}>
              Uploading... {uploadProgress}%
            </p>
            <div style={{
              width: '200px',
              height: '6px',
              background: '#e2e8f0',
              borderRadius: '3px',
              margin: '0 auto',
              overflow: 'hidden',
            }}>
              <div style={{
                width: `${uploadProgress}%`,
                height: '100%',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                transition: 'width 0.3s',
              }} />
            </div>
          </div>
        ) : (
          <>
            <div style={{
              fontSize: '48px',
              marginBottom: '20px',
            }}>
              📸
            </div>
            <h3 style={{
              fontSize: '20px',
              fontWeight: '600',
              color: '#1a202c',
              margin: '0 0 10px 0',
            }}>
              {dragActive ? 'Drop your photos here' : 'Upload Photos'}
            </h3>
            <p style={{
              fontSize: '14px',
              color: '#718096',
              margin: '0 0 20px 0',
            }}>
              Drag and drop your photos here, or click to browse
            </p>
            <button
              style={{
                padding: '12px 32px',
                background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                color: 'white',
                border: 'none',
                borderRadius: '12px',
                fontWeight: '600',
                fontSize: '14px',
                cursor: 'pointer',
                transition: 'transform 0.2s',
                boxShadow: '0 4px 12px rgba(102, 126, 234, 0.4)',
              }}
              onClick={(e) => {
                e.stopPropagation();
                handleClick();
              }}
            >
              Choose Files
            </button>
          </>
        )}

        <style>
          {`
            @keyframes spin {
              to { transform: rotate(360deg); }
            }
          `}
        </style>
      </div>
    </div>
  );
}
