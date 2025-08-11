import React, { useEffect, useState } from 'react';
import {
  Box,
  TextField,
  MenuItem,
  InputLabel,
  FormControl,
  Select,
  OutlinedInput,
  Checkbox,
  ListItemText,
  Button,
  Alert,
  CircularProgress
} from '@mui/material';
import { receiptsApi } from '../api';

const ITEM_HEIGHT = 48;
const ITEM_PADDING_TOP = 8;
const MenuProps = {
  PaperProps: {
    style: { maxHeight: ITEM_HEIGHT * 4.5 + ITEM_PADDING_TOP, width: 250 },
  },
};

export default function ReceiptsFilters({ filters, setFilters, onApply, reloadKey }) {
  const [numbers, setNumbers] = useState([]);
  const [resources, setResources] = useState([]);
  const [units, setUnits] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchFilterData = async () => {
      setLoading(true);
      setError(null);
      try {
        const [numbersRes, resourcesRes, unitsRes] = await Promise.all([
          receiptsApi.numbers(),
          receiptsApi.resources(),
          receiptsApi.units()
        ]);
        setNumbers(numbersRes.map(item => item.number ?? item));
        setResources(resourcesRes);
        setUnits(unitsRes);
      } catch (err) {
        setError('Ошибка загрузки данных для фильтров');
      } finally {
        setLoading(false);
      }
    };
    fetchFilterData();
  }, [reloadKey]);

  if (loading) return (<Box sx={{ display: 'flex', justifyContent: 'center', p: 2 }}><CircularProgress /></Box>);
  if (error) return (<Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>);

  return (
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 2 }}>
      <TextField label="С даты" type="date" value={filters.from} onChange={e => setFilters(f => ({ ...f, from: e.target.value }))} InputLabelProps={{ shrink: true }} size="small" />
      <TextField label="По дату" type="date" value={filters.to} onChange={e => setFilters(f => ({ ...f, to: e.target.value }))} InputLabelProps={{ shrink: true }} size="small" />
      <FormControl sx={{ minWidth: 200 }} size="small">
        <InputLabel>Номера документов</InputLabel>
        <Select multiple value={filters.numbers} onChange={e => setFilters(f => ({ ...f, numbers: e.target.value }))} input={<OutlinedInput label="Номера документов" />} renderValue={selected => selected.join(', ')} MenuProps={MenuProps}>
          {numbers.map((num) => (
            <MenuItem key={num} value={num}>
              <Checkbox checked={filters.numbers.indexOf(num) > -1} />
              <ListItemText primary={num} />
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <FormControl sx={{ minWidth: 200 }} size="small">
        <InputLabel>Ресурсы</InputLabel>
        <Select multiple value={filters.resourceIds} onChange={e => setFilters(f => ({ ...f, resourceIds: e.target.value }))} input={<OutlinedInput label="Ресурсы" />} renderValue={selected => resources.filter(r => selected.includes(r.id)).map(r => r.name).join(', ')} MenuProps={MenuProps}>
          {resources.map((r) => (
            <MenuItem key={r.id} value={r.id}>
              <Checkbox checked={filters.resourceIds.indexOf(r.id) > -1} />
              <ListItemText primary={r.name} />
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <FormControl sx={{ minWidth: 200 }} size="small">
        <InputLabel>Единицы измерения</InputLabel>
        <Select multiple value={filters.unitIds} onChange={e => setFilters(f => ({ ...f, unitIds: e.target.value }))} input={<OutlinedInput label="Единицы измерения" />} renderValue={selected => units.filter(u => selected.includes(u.id)).map(u => u.name).join(', ')} MenuProps={MenuProps}>
          {units.map((u) => (
            <MenuItem key={u.id} value={u.id}>
              <Checkbox checked={filters.unitIds.indexOf(u.id) > -1} />
              <ListItemText primary={u.name} />
            </MenuItem>
          ))}
        </Select>
      </FormControl>
      <Button variant="contained" onClick={onApply} sx={{ alignSelf: 'center' }}>Применить</Button>
    </Box>
  );
}


