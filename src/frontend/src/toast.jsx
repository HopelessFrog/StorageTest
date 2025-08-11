import React from 'react';
import { Snackbar, Alert } from '@mui/material';

const EVENT_NAME = 'app-error';

export function notifyError(message) {
  if (!message) return;
  window.dispatchEvent(new CustomEvent(EVENT_NAME, { detail: String(message) }));
}

export function Toasts() {
  const [queue, setQueue] = React.useState([]);
  const [open, setOpen] = React.useState(false);
  const [current, setCurrent] = React.useState('');

  React.useEffect(() => {
    const handler = (e) => {
      setQueue((q) => [...q, e.detail]);
    };
    window.addEventListener(EVENT_NAME, handler);
    return () => window.removeEventListener(EVENT_NAME, handler);
  }, []);

  React.useEffect(() => {
    if (!open && queue.length > 0) {
      setCurrent(queue[0]);
      setQueue((q) => q.slice(1));
      setOpen(true);
    }
  }, [queue, open]);

  const handleClose = (_, reason) => {
    if (reason === 'clickaway') return;
    setOpen(false);
  };

  return (
    <Snackbar open={open} autoHideDuration={4000} onClose={handleClose} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
      <Alert onClose={handleClose} severity="error" variant="filled" sx={{ width: '100%' }}>
        {current}
      </Alert>
    </Snackbar>
  );
}


