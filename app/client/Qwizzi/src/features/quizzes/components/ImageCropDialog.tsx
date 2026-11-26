import { useEffect, useState } from 'react';
import { ImagePlus, LoaderCircle, MoveHorizontal, MoveVertical, X } from 'lucide-react';
import { ApiError } from '../../../core/api/apiError';
import { apiRequest } from '../../../core/api/httpClient';
import type { ImageAsset } from '../api/quizzesService';

interface ImageCropDialogProps {
  onClose: () => void;
  onUploaded: (image: ImageAsset) => void;
}

export function ImageCropDialog({ onClose, onUploaded }: ImageCropDialogProps) {
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState('');
  const [zoom, setZoom] = useState(1);
  const [horizontal, setHorizontal] = useState(50);
  const [vertical, setVertical] = useState(50);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!previewUrl) return;
    return () => URL.revokeObjectURL(previewUrl);
  }, [previewUrl]);

  async function uploadCrop() {
    if (!file || !previewUrl) return;
    setBusy(true);
    setError('');
    try {
      const source = new Image();
      source.src = previewUrl;
      await source.decode();
      const cropSize = Math.min(source.naturalWidth, source.naturalHeight) / zoom;
      const sourceX = (source.naturalWidth - cropSize) * horizontal / 100;
      const sourceY = (source.naturalHeight - cropSize) * vertical / 100;
      const canvas = document.createElement('canvas');
      canvas.width = 512;
      canvas.height = 512;
      const context = canvas.getContext('2d');
      if (!context) throw new Error('Your browser could not prepare the image.');
      context.drawImage(source, sourceX, sourceY, cropSize, cropSize, 0, 0, 512, 512);
      const blob = await new Promise<Blob>((resolve, reject) => {
        canvas.toBlob((result) => result ? resolve(result) : reject(new Error('Could not resize the image.')), 'image/jpeg', 0.88);
      });
      const body = new FormData();
      body.append('file', blob, `${file.name.replace(/\.[^.]+$/, '') || 'image'}.jpg`);
      const result = await apiRequest<ImageAsset>('/images', { method: 'POST', body });
      onUploaded(result);
    } catch (requestError) {
      setError(requestError instanceof ApiError
        ? requestError.message
        : requestError instanceof Error ? requestError.message : 'Unable to process this image.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="modal-backdrop crop-modal-backdrop" onMouseDown={onClose}>
      <section
        className="image-crop-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="image-crop-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className="modal-heading">
          <div><span className="eyebrow">IMAGE EDITOR</span><h2 id="image-crop-title">Crop and resize</h2></div>
          <button type="button" className="icon-button" aria-label="Close image editor" onClick={onClose}><X size={20} /></button>
        </div>
        <p className="crop-intro">Choose a square crop. Your image is resized to 512 × 512 pixels before upload.</p>
        <label className="image-file-picker">
          <ImagePlus size={19} aria-hidden="true" />
          <span>{file ? file.name : 'Choose an image'}</span>
          <input
            type="file"
            accept="image/jpeg,image/png,image/webp"
            onChange={(event) => {
              const nextFile = event.target.files?.[0];
              if (!nextFile) return;
              if (nextFile.size > 12 * 1024 * 1024) {
                setError('Choose an image smaller than 12 MB.');
                return;
              }
              setError('');
              setPreviewUrl(URL.createObjectURL(nextFile));
              setFile(nextFile);
            }}
          />
        </label>
        {previewUrl && (
          <div className="crop-workspace">
            <div className="crop-preview-frame">
              <img src={previewUrl} alt="Image crop preview" style={{ objectPosition: `${horizontal}% ${vertical}%`, transform: `scale(${zoom})` }} />
            </div>
            <div className="crop-controls">
              <label>Zoom <input type="range" min="1" max="3" step="0.05" value={zoom} onChange={(event) => setZoom(Number(event.target.value))} /></label>
              <label><MoveHorizontal size={15} aria-hidden="true" /> Horizontal crop <input type="range" min="0" max="100" value={horizontal} onChange={(event) => setHorizontal(Number(event.target.value))} /></label>
              <label><MoveVertical size={15} aria-hidden="true" /> Vertical crop <input type="range" min="0" max="100" value={vertical} onChange={(event) => setVertical(Number(event.target.value))} /></label>
            </div>
          </div>
        )}
        {error && <div className="notice error-notice" role="alert">{error}</div>}
        <div className="modal-actions">
          <button type="button" className="button button-outline" onClick={onClose} disabled={busy}>Cancel</button>
          <button type="button" className="button button-primary" onClick={() => void uploadCrop()} disabled={!file || busy}>
            {busy ? <LoaderCircle className="spin" size={17} /> : <ImagePlus size={17} />}
            {busy ? 'Preparing…' : 'Use this image'}
          </button>
        </div>
      </section>
    </div>
  );
}
