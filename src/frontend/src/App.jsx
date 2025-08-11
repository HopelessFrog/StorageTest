import React from 'react';
import { AppBar, Tabs, Tab, Box, CssBaseline } from '@mui/material';
import ReceiptLongIcon from '@mui/icons-material/ReceiptLong';
import InventoryIcon from '@mui/icons-material/Inventory';
import ScaleIcon from '@mui/icons-material/Scale';
import ReceiptsFilters from './components/ReceiptsFilters';
import ReceiptsTable from './components/ReceiptsTable';
import UnitsTable from './components/UnitsTable';
import ResourcesTable from './components/ResourcesTable';
import { receiptsApi } from './api';

function TabPanel(props) {
  const { children, value, index, ...other } = props;
  return (
    <div role="tabpanel" hidden={value !== index} id={`tabpanel-${index}`} aria-labelledby={`tab-${index}`} {...other}>
      {value === index && (<Box sx={{ p: 3 }}>{children}</Box>)}
    </div>
  );
}

function a11yProps(index) { return { id: `tab-${index}`, 'aria-controls': `tabpanel-${index}` }; }

export default function App() {
  const [value, setValue] = React.useState(0);
  const [loading, setLoading] = React.useState(false);
  const [error, setError] = React.useState(null);
  const [filters, setFilters] = React.useState({ from: '', to: '', numbers: [], resourceIds: [], unitIds: [] });
  const [appliedFilters, setAppliedFilters] = React.useState({ from: '', to: '', numbers: [], resourceIds: [], unitIds: [] });
  const [filtersReloadKey, setFiltersReloadKey] = React.useState(0);
  const [receiptsPage, setReceiptsPage] = React.useState({ items: [], page: 1, pageSize: 10, totalItems: 0 });
  const [receiptsPagination, setReceiptsPagination] = React.useState({ page: 1, pageSize: 10 });

  const handleChange = (event, newValue) => { setValue(newValue); };

  const toUtcStartOfDay = (dateStr) => { if (!dateStr) return undefined; const d = new Date(dateStr); return new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(), 0, 0, 0, 0)).toISOString(); };
  const toUtcEndOfDay = (dateStr) => { if (!dateStr) return undefined; const d = new Date(dateStr); return new Date(Date.UTC(d.getFullYear(), d.getMonth(), d.getDate(), 23, 59, 59, 999)).toISOString(); };
  const buildQuery = () => ({
    From: toUtcStartOfDay(appliedFilters.from),
    To: toUtcEndOfDay(appliedFilters.to || appliedFilters.from),
    Numbers: appliedFilters.numbers || [],
    ResourceIds: appliedFilters.resourceIds || [],
    UnitIds: appliedFilters.unitIds || [],
    Page: receiptsPagination.page,
    PageSize: receiptsPagination.pageSize
  });

  const fetchReceipts = async () => {
    if (!filters.from) return;
    setLoading(true); setError(null);
    try { setReceiptsPage(await receiptsApi.get(buildQuery())); }
    catch { setError('Ошибка загрузки данных'); setReceiptsPage({ items: [], page: 1, pageSize: receiptsPagination.pageSize, totalItems: 0 }); }
    finally { setLoading(false); }
  };

  React.useEffect(() => {
    const now = new Date();
    const firstDayPrevYear = new Date(now.getFullYear() - 1, now.getMonth(), 1).toISOString().slice(0, 10);
    const lastDayThisMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0).toISOString().slice(0, 10);
    const initial = { from: firstDayPrevYear, to: lastDayThisMonth, numbers: [], resourceIds: [], unitIds: [] };
    setFilters(initial);
    setAppliedFilters(initial);
  }, []);
  React.useEffect(() => { if (appliedFilters.from) { fetchReceipts(); } }, [appliedFilters, receiptsPagination]);
  const handleApplyFilters = () => { setReceiptsPagination(p => ({ ...p, page: 1 })); setAppliedFilters(filters); };

  return (
    <Box sx={{ flexGrow: 1 }}>
      <CssBaseline />
      <AppBar position="static" color="default" elevation={1}>
        <Tabs value={value} onChange={handleChange} centered variant="fullWidth" textColor="primary" indicatorColor="primary" sx={{ '& .MuiTab-root': { minHeight: 56 }, '& .Mui-selected': { fontWeight: 600 } }}>
          <Tab icon={<ReceiptLongIcon />} label="Поступления" {...a11yProps(0)} />
          <Tab icon={<ScaleIcon />} label="Единицы измерения" {...a11yProps(1)} />
          <Tab icon={<InventoryIcon />} label="Ресурсы" {...a11yProps(2)} />
        </Tabs>
      </AppBar>
      <TabPanel value={value} index={0}>
        <ReceiptsFilters filters={filters} setFilters={setFilters} onApply={handleApplyFilters} reloadKey={filtersReloadKey} />
        {loading && <Box sx={{ p: 2 }}>Загрузка данных...</Box>}
        {error && <Box sx={{ p: 2, color: 'error.main' }}>{error}</Box>}
        {!loading && !error && (
          <ReceiptsTable
            receipts={receiptsPage.items}
            page={receiptsPagination.page}
            pageSize={receiptsPagination.pageSize}
            totalItems={receiptsPage.totalItems}
            onPageChange={(page) => setReceiptsPagination(p => ({ ...p, page }))}
            onPageSizeChange={(pageSize) => setReceiptsPagination(p => ({ ...p, page: 1, pageSize }))}
            onRefresh={fetchReceipts}
            onFiltersRefresh={() => setFiltersReloadKey(k => k + 1)}
          />
        )}
      </TabPanel>
      <TabPanel value={value} index={1}>
        <UnitsTable />
      </TabPanel>
      <TabPanel value={value} index={2}>
        <ResourcesTable />
      </TabPanel>
    </Box>
  );
}


