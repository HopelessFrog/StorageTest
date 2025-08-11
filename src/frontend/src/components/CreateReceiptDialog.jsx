import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  FormControl,
  Select,
  MenuItem,
  Grid,
  Alert
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import DeleteIcon from '@mui/icons-material/Delete';
import { receiptsApi, resourcesApi, unitsApi } from '../api';

export default function CreateReceiptDialog({ open, onClose, onSuccess, initialData }) {
  const [formData, setFormData] = useState({ id: null, number: '', date: new Date().toISOString().slice(0, 10) });
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [incomeResources, setIncomeResources] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const fetchData = async () => {
    try {
      const [resAct, resArch, unitAct, unitArch] = await Promise.all([
        resourcesApi.list({ State: 'Active', PageSize: 100 }),
        resourcesApi.list({ State: 'Archived', PageSize: 100 }),
        unitsApi.list({ State: 'Active', PageSize: 100 }),
        unitsApi.list({ State: 'Archived', PageSize: 100 })
      ]);
      const mark = (arr, isArchived) => (arr ?? []).map(x => ({ ...x, isArchived }));
      const mergeUniquePreferActive = (active, archived) => {
        const map = new Map();
        active.forEach(x => map.set(x.id, x));
        archived.forEach(x => { if (!map.has(x.id)) map.set(x.id, x); });
        return Array.from(map.values());
      };
      setResources(mergeUniquePreferActive(mark(resAct.items, false), mark(resArch.items, true)));
      setUnits(mergeUniquePreferActive(mark(unitAct.items, false), mark(unitArch.items, true)));
    } catch {}
  };

  useEffect(() => {
    if (!open) return;
    fetchData();
    if (initialData) {
      setFormData({ id: initialData.id, number: initialData.number, date: new Date(initialData.date).toISOString().slice(0, 10) });
      (async () => {
        try {
          const res = await receiptsApi.incomeResources(initialData.id);
          setIncomeResources(res.map(r => ({ id: r.id, resourceId: Number(r.resourceId), unitId: Number(r.unitId), quantity: r.quantity })));
        } catch { setIncomeResources([]); }
      })();
    } else {
      setFormData({ id: null, number: '', date: new Date().toISOString().slice(0, 10) });
      setIncomeResources([]);
    }
  }, [open, initialData]);

  const handleAddResource = () => { setIncomeResources([...incomeResources, { resourceId: '', unitId: '', quantity: 1 }]); };
  const handleRemoveResource = (index) => { setIncomeResources(incomeResources.filter((_, i) => i !== index)); };
  const handleResourceChange = (index, field, value) => {
    const updated = [...incomeResources];
    let parsedValue;
    if (field === 'quantity') {
      parsedValue = value; 
    } else {
      parsedValue = parseInt(value, 10) || '';
    }
    updated[index] = { ...updated[index], [field]: parsedValue };
    setIncomeResources(updated);
  };

  const handleSubmit = async () => {
    setError(null);
    if (!formData.number) { setError('Заполните номер документа'); return; }
    if (incomeResources.length > 0) {
      const incompleteResources = incomeResources.filter(ir => !ir.resourceId || !ir.unitId || !ir.quantity || ir.quantity <= 0);
      if (incompleteResources.length > 0) { setError('Заполните все поля для всех ресурсов'); return; }
    }
    setLoading(true);
    try {
      const payload = {
        number: formData.number,
        date: new Date(formData.date + 'T00:00:00.000Z').toISOString(),
        incomeResources: incomeResources.map(ir => {
          const q = parseFloat(ir.quantity);
          const quantity = Number.isFinite(q) ? Math.round(q * 1000) / 1000 : 0;
          return {
            id: ir.id ?? null,
            resourceId: parseInt(ir.resourceId, 10),
            unitId: parseInt(ir.unitId, 10),
            quantity
          };
        })
      };
      if (formData.id) await receiptsApi.update({ id: formData.id, ...payload }); else await receiptsApi.create(payload);
      onSuccess();
      handleClose();
    } catch (error) { setError(error.response?.data?.message || 'Ошибка при сохранении поступления'); }
    finally { setLoading(false); }
  };

  const handleClose = () => { setFormData({ id: null, number: '', date: new Date().toISOString().slice(0, 10) }); setIncomeResources([]); setError(null); onClose(); };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>{formData.id ? 'Редактировать поступление' : 'Создать новое поступление'}</DialogTitle>
      <DialogContent>
        <Box sx={{ mb: 3 }}>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField label="Номер документа" fullWidth value={formData.number} onChange={(e) => setFormData({ ...formData, number: e.target.value })} margin="normal" />
            </Grid>
            <Grid item xs={6}>
              <TextField label="Дата" type="date" fullWidth value={formData.date} onChange={(e) => setFormData({ ...formData, date: e.target.value })} margin="normal" InputLabelProps={{ shrink: true }} />
            </Grid>
          </Grid>
        </Box>
        <Box sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="h6">Ресурсы</Typography>
            <Button variant="outlined" startIcon={<AddIcon />} onClick={handleAddResource}>Добавить ресурс</Button>
          </Box>
          {incomeResources.length === 0 ? (
            <Typography variant="body2" color="textSecondary">Добавьте ресурсы для поступления</Typography>
          ) : (
            <TableContainer component={Paper}>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Ресурс</TableCell>
                    <TableCell>Единица измерения</TableCell>
                    <TableCell>Количество</TableCell>
                    <TableCell>Действия</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(() => {
                    const usedResourceIds = new Set(incomeResources.map(ir => Number(ir.resourceId)).filter(Boolean));
                    const usedUnitIds = new Set(incomeResources.map(ir => Number(ir.unitId)).filter(Boolean));
                    return incomeResources.map((item, index) => (
                      <TableRow key={item.id ?? index}>
                        <TableCell>
                          <FormControl fullWidth size="small">
                            <Select value={item.resourceId} onChange={(e) => handleResourceChange(index, 'resourceId', e.target.value)}>
                              {resources.filter(r => !r.isArchived || usedResourceIds.has(r.id) || r.id === item.resourceId).map((resource) => (
                                <MenuItem key={resource.id} value={resource.id}>{resource.name}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </TableCell>
                        <TableCell>
                          <FormControl fullWidth size="small">
                            <Select value={item.unitId} onChange={(e) => handleResourceChange(index, 'unitId', e.target.value)}>
                              {units.filter(u => !u.isArchived || usedUnitIds.has(u.id) || u.id === item.unitId).map((unit) => (
                                <MenuItem key={unit.id} value={unit.id}>{unit.name}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </TableCell>
                        <TableCell>
                        <TextField type="number" size="small" value={item.quantity} onChange={(e) => handleResourceChange(index, 'quantity', e.target.value)} inputProps={{ min: 0.001, step: 0.001 }} />
                        </TableCell>
                        <TableCell>
                          <IconButton onClick={() => handleRemoveResource(index)}><DeleteIcon /></IconButton>
                        </TableCell>
                      </TableRow>
                    ));
                  })()}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </Box>
        {error && (<Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>)}
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose}>Отмена</Button>
        <Button onClick={handleSubmit} variant="contained" disabled={loading}>{loading ? (formData.id ? 'Сохранение...' : 'Создание...') : (formData.id ? 'Сохранить' : 'Создать')}</Button>
      </DialogActions>
    </Dialog>
  );
}


