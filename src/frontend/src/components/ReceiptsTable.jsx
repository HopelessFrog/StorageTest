import React, { useEffect, useState } from 'react';
import { 
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, IconButton, Button, Typography, Box, Stack, TextField
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import { receiptsApi } from '../api';
import CreateReceiptDialog from './CreateReceiptDialog';
import ConfirmDialog from './ConfirmDialog';

export default function ReceiptsTable({ receipts, page, pageSize, totalItems, onPageChange, onPageSizeChange, onRefresh, onFiltersRefresh }) {
  const [resourcesByReceipt, setResourcesByReceipt] = useState({});
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [confirmDelete, setConfirmDelete] = useState(null);

  useEffect(() => {
    if (!receipts || receipts.length === 0) { setResourcesByReceipt({}); return; }
    const map = Object.fromEntries(
      receipts.map(r => [r.id, Array.isArray(r.incomeResources) ? r.incomeResources : []])
    );
    setResourcesByReceipt(map);
  }, [receipts]);

  const handleDelete = async (id) => { try { await receiptsApi.remove(id); onRefresh?.(); onFiltersRefresh?.(); } catch (e) {} };

  const renderReceiptRows = (receipt) => {
    const resources = resourcesByReceipt[receipt.id] || [];
    const rowsCount = Math.max(resources.length, 1);

    if (resources.length === 0) {
      return (
        <TableRow key={`${receipt.id}-empty`}>
          <TableCell rowSpan={1}>
            <Stack direction="row" spacing={1} alignItems="center">
              <span>{receipt.number}</span>
              <IconButton size="small" onClick={() => { setEditing(receipt); setCreateDialogOpen(true); }}>
                <EditIcon fontSize="inherit" />
              </IconButton>
              <IconButton size="small" onClick={() => setConfirmDelete(receipt)}>
                <DeleteIcon fontSize="inherit" />
              </IconButton>
            </Stack>
          </TableCell>
          <TableCell rowSpan={1}>{new Date(receipt.date).toLocaleDateString()}</TableCell>
          <TableCell />
          <TableCell />
          <TableCell>0</TableCell>
        </TableRow>
      );
    }

    return resources.map((r, idx) => (
      <TableRow key={`${receipt.id}-${r.id}`}>
        {idx === 0 && (
          <TableCell rowSpan={rowsCount}>
            <Stack direction="row" spacing={1} alignItems="center">
              <span>{receipt.number}</span>
              <IconButton size="small" onClick={() => { setEditing(receipt); setCreateDialogOpen(true); }}>
                <EditIcon fontSize="inherit" />
              </IconButton>
              <IconButton size="small" onClick={() => setConfirmDelete(receipt)}>
                <DeleteIcon fontSize="inherit" />
              </IconButton>
            </Stack>
          </TableCell>
        )}
        {idx === 0 && (<TableCell rowSpan={rowsCount}>{new Date(receipt.date).toLocaleDateString()}</TableCell>)}
         <TableCell>{r.resource}</TableCell>
         <TableCell>{r.unit}</TableCell>
         <TableCell>{r.quantity}</TableCell>
      </TableRow>
    ));
  };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">Список поступлений</Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => { setEditing(null); setCreateDialogOpen(true); }}>Добавить поступление</Button>
        </Box>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Номер</TableCell>
              <TableCell>Дата</TableCell>
              <TableCell>Ресурс</TableCell>
              <TableCell>Единица измерения</TableCell>
              <TableCell>Количество</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {receipts.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  <Typography variant="body2" color="textSecondary">Нет данных для отображения</Typography>
                </TableCell>
              </TableRow>
            ) : (receipts.flatMap((row) => renderReceiptRows(row)))}
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1 }}>
        <Typography variant="body2">Всего: {totalItems}</Typography>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Button disabled={page <= 1} onClick={() => onPageChange?.(page - 1)}>Назад</Button>
          <Typography variant="body2">Стр. {page}</Typography>
          <Button disabled={(page * pageSize) >= totalItems} onClick={() => onPageChange?.(page + 1)}>Вперед</Button>
          <TextField
            select
            label="На странице"
            value={pageSize}
            onChange={(e) => onPageSizeChange?.(parseInt(e.target.value, 10))}
            size="small"
            SelectProps={{ native: true }}
            sx={{ width: 120 }}
          >
            {[10, 20, 50, 100].map(sz => <option key={sz} value={sz}>{sz}</option>)}
          </TextField>
        </Box>
      </Box>

      <CreateReceiptDialog open={createDialogOpen} onClose={() => setCreateDialogOpen(false)} onSuccess={() => { setCreateDialogOpen(false); onRefresh?.(); onFiltersRefresh?.(); }} initialData={editing} />

      <ConfirmDialog
        open={!!confirmDelete}
        title="Удалить поступление"
        message={`Вы уверены, что хотите удалить документ ${confirmDelete?.number}?`}
        confirmText="Удалить"
        cancelText="Отмена"
        onClose={() => setConfirmDelete(null)}
        onConfirm={() => { if (confirmDelete) { handleDelete(confirmDelete.id); setConfirmDelete(null); } }}
      />
    </>
  );
}


