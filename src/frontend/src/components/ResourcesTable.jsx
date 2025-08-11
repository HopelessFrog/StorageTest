import React, { useState, useEffect } from 'react';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, IconButton, Dialog, DialogTitle, DialogContent, DialogActions, Button, TextField, Box, Typography, CircularProgress, Alert, ToggleButton, ToggleButtonGroup } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import ArchiveIcon from '@mui/icons-material/Archive';
import UnarchiveIcon from '@mui/icons-material/Unarchive';
import { resourcesApi } from '../api';
import ConfirmDialog from './ConfirmDialog';

export default function ResourcesTable() {
  const [pageData, setPageData] = useState({ items: [], page: 1, pageSize: 10, totalItems: 0 });
  const [pagination, setPagination] = useState({ page: 1, pageSize: 10 });
  const [open, setOpen] = useState(false);
  const [editingResource, setEditingResource] = useState(null);
  const [formData, setFormData] = useState({ name: '' });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [archiveState, setArchiveState] = useState('Active');
  const [confirmDelete, setConfirmDelete] = useState(null);

  useEffect(() => { fetchResources(); }, [archiveState, pagination]);

  const fetchResources = async () => {
    setLoading(true); setError(null);
    try { setPageData(await resourcesApi.list({ State: archiveState, Page: pagination.page, PageSize: pagination.pageSize })); }
    catch { setError('Ошибка загрузки ресурсов'); setPageData({ items: [], page: 1, pageSize: pagination.pageSize, totalItems: 0 }); }
    finally { setLoading(false); }
  };

  const handleOpen = (resource = null) => { if (resource) { setEditingResource(resource); setFormData({ name: resource.name }); } else { setEditingResource(null); setFormData({ name: '' }); } setOpen(true); };
  const handleClose = () => { setOpen(false); setEditingResource(null); setFormData({ name: '' }); };

  const handleSubmit = async () => { try { if (editingResource) await resourcesApi.update({ id: editingResource.id, ...formData }); else await resourcesApi.create(formData); fetchResources(); handleClose(); } catch {} };
  const handleDelete = async (id) => { try { await resourcesApi.remove(id); fetchResources(); } catch {} };
  const handleArchive = async (id) => { try { await resourcesApi.archive(id); fetchResources(); } catch {} };
  const handleUnarchive = async (id) => { try { await resourcesApi.unarchive(id); fetchResources(); } catch {} };

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">Ресурсы</Typography>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <ToggleButtonGroup value={archiveState} exclusive onChange={(e, v) => v && setArchiveState(v)} size="small">
            <ToggleButton value="Active">Активные</ToggleButton>
            <ToggleButton value="Archived">Архив</ToggleButton>
          </ToggleButtonGroup>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => handleOpen()} disabled={archiveState === 'Archived'}>Добавить</Button>
        </Box>
      </Box>

      {loading && (<Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}><CircularProgress /></Box>)}
      {error && (<Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>)}

      {!loading && !error && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Название</TableCell>
                <TableCell>Действия</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {pageData.items.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={2} align="center"><Typography variant="body2" color="textSecondary">Нет данных для отображения</Typography></TableCell>
                </TableRow>
              ) : (
                pageData.items.map((resource) => (
                  <TableRow key={resource.id}>
                    <TableCell>{resource.name}</TableCell>
                    <TableCell>
                      <IconButton onClick={() => handleOpen(resource)}><EditIcon /></IconButton>
                      {archiveState === 'Active' ? (
                        <IconButton onClick={() => handleArchive(resource.id)}><ArchiveIcon /></IconButton>
                      ) : (
                        <IconButton onClick={() => handleUnarchive(resource.id)}><UnarchiveIcon /></IconButton>
                      )}
                      <IconButton onClick={() => setConfirmDelete(resource)}><DeleteIcon /></IconButton>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1 }}>
        <Typography variant="body2">Всего: {pageData.totalItems}</Typography>
        <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
          <Button disabled={pagination.page <= 1} onClick={() => setPagination(p => ({ ...p, page: p.page - 1 }))}>Назад</Button>
          <Typography variant="body2">Стр. {pageData.page}</Typography>
          <Button disabled={(pageData.page * pageData.pageSize) >= pageData.totalItems} onClick={() => setPagination(p => ({ ...p, page: p.page + 1 }))}>Вперед</Button>
          <TextField
            select
            label="На странице"
            value={pagination.pageSize}
            onChange={(e) => setPagination({ page: 1, pageSize: parseInt(e.target.value, 10) })}
            size="small"
            SelectProps={{ native: true }}
            sx={{ width: 120 }}
          >
            {[10, 20, 50, 100].map(sz => <option key={sz} value={sz}>{sz}</option>)}
          </TextField>
        </Box>
      </Box>

      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>{editingResource ? 'Редактировать ресурс' : 'Добавить ресурс'}</DialogTitle>
        <DialogContent>
          <TextField autoFocus margin="dense" label="Название" fullWidth variant="outlined" value={formData.name} onChange={(e) => setFormData({ name: e.target.value })} />
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>Отмена</Button>
          <Button onClick={handleSubmit} variant="contained">{editingResource ? 'Сохранить' : 'Добавить'}</Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={!!confirmDelete}
        title="Удалить ресурс"
        message={`Вы уверены, что хотите удалить ресурс ${confirmDelete?.name}?`}
        confirmText="Удалить"
        cancelText="Отмена"
        onClose={() => setConfirmDelete(null)}
        onConfirm={() => { if (confirmDelete) { handleDelete(confirmDelete.id); setConfirmDelete(null); } }}
      />
    </>
  );
}


