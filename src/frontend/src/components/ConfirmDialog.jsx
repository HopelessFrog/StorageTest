import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Divider,
  Avatar
} from '@mui/material';
import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded';

export default function ConfirmDialog({
  open,
  title = 'Подтверждение',
  message,
  confirmText = 'Удалить',
  cancelText = 'Отмена',
  onConfirm,
  onClose,
  confirmColor = 'error'
}) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, px: 3, pt: 2 }}>
        <Avatar sx={{ bgcolor: `${confirmColor}.light`, color: `${confirmColor}.main`, width: 36, height: 36 }}>
          <WarningAmberRoundedIcon />
        </Avatar>
        <DialogTitle sx={{ px: 0, py: 0 }}>{title}</DialogTitle>
      </Box>
      <DialogContent sx={{ px: 3 }}>
        <Typography variant="body1" sx={{ mt: 1 }}>{message}</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          Это действие необратимо.
        </Typography>
      </DialogContent>
      <Divider />
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose}>{cancelText}</Button>
        <Button variant="contained" color={confirmColor} onClick={onConfirm}>{confirmText}</Button>
      </DialogActions>
    </Dialog>
  );
}


