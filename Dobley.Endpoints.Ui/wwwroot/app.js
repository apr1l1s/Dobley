const state = {
    accessToken: localStorage.getItem('dobley.accessToken') || '',
    refreshToken: localStorage.getItem('dobley.refreshToken') || '',
    selectedStorageId: Number(localStorage.getItem('dobley.selectedStorageId')) || null,
    activeTab: localStorage.getItem('dobley.activeTab') || 'storages',
    storages: [],
    products: [],
    telegramBotUserName: window.DobleyUiConfig?.telegramBotUserName || ''
};

let categories = [];
let unitTypes = [];

const byId = id => document.getElementById(id);

const elements = {
    loginFormFields: byId('login-form-fields'),
    authOnlineSummary: byId('auth-online-summary'),
    authStatus: byId('auth-status'),
    login: byId('login'),
    logoutButton: byId('logout-button'),
    password: byId('password'),
    productStorageSelect: byId('product-storage-select'),
    storagesTab: byId('storages-tab'),
    productsTab: byId('products-tab'),
    storagesTabButton: byId('storages-tab-button'),
    productsTabButton: byId('products-tab-button'),
    storagesList: byId('storages-list'),
    productsList: byId('products-list'),
    selectedStorageLabel: byId('selected-storage-label'),
    toast: byId('toast')
};

function authHeaders(contentType = false) {
    const headers = { Authorization: `Bearer ${state.accessToken}` };
    if (contentType) {
        headers['Content-Type'] = 'application/json';
    }
    return headers;
}

async function request(path, options = {}) {
    const response = await fetch(path, options);
    if (response.ok) {
        if (response.status === 204) {
            return null;
        }

        const text = await response.text();
        if (!text) {
            return null;
        }

        try {
            return JSON.parse(text);
        } catch {
            return text;
        }
    }

    let message = `HTTP ${response.status}`;
    try {
        const body = await response.json();
        message = body.error || body.title || message;
    } catch {
        message = await response.text() || message;
    }

    throw new Error(message);
}

function showToast(message) {
    elements.toast.textContent = message;
    elements.toast.classList.add('visible');
    window.clearTimeout(showToast.timer);
    showToast.timer = window.setTimeout(() => elements.toast.classList.remove('visible'), 4200);
}

function setAuthStatus() {
    const isOnline = Boolean(state.accessToken);
    elements.authStatus.textContent = isOnline ? 'online' : 'offline';
    elements.authStatus.classList.toggle('online', isOnline);
    elements.loginFormFields.classList.toggle('hidden', isOnline);
    elements.authOnlineSummary.classList.toggle('hidden', !isOnline);
    elements.logoutButton.classList.toggle('hidden', !isOnline);
}

function setActiveTab(tab) {
    state.activeTab = tab;
    localStorage.setItem('dobley.activeTab', tab);
    renderTabs();
}

function renderTabs() {
    const isStoragesTab = state.activeTab === 'storages';
    elements.storagesTab.classList.toggle('hidden', !isStoragesTab);
    elements.productsTab.classList.toggle('hidden', isStoragesTab);
    elements.storagesTabButton.classList.toggle('active', isStoragesTab);
    elements.productsTabButton.classList.toggle('active', !isStoragesTab);
}

function fillSelect(select, values) {
    const selectedValue = select.value;
    const options = values.map(value => {
        const optionValue = read(value, 'name') || value;
        const option = document.createElement('option');
        option.value = optionValue;
        option.textContent = read(value, 'displayName') || optionValue;
        option.selected = optionValue === selectedValue;
        return option;
    });

    if (options.length === 0) {
        const option = document.createElement('option');
        option.value = '';
        option.textContent = 'Справочник не загружен';
        options.push(option);
    }

    select.replaceChildren(...options);
}

function getAuthCredentials() {
    return {
        login: elements.login.value.trim(),
        password: elements.password.value
    };
}

async function login(options = {}) {
    const body = getAuthCredentials();
    const tokens = await request('/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    state.accessToken = read(tokens, 'accessToken');
    state.refreshToken = read(tokens, 'refreshToken');
    localStorage.setItem('dobley.accessToken', state.accessToken);
    localStorage.setItem('dobley.refreshToken', state.refreshToken);
    setAuthStatus();
    await loadAll();
    if (options.showSuccess !== false) {
        showToast('Вход выполнен.');
    }
}

async function register() {
    const body = getAuthCredentials();
    await request('/auth/reg', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(body)
    });

    await login({ showSuccess: false });
    showToast('Регистрация выполнена, вход выполнен.');
}

function logout() {
    state.accessToken = '';
    state.refreshToken = '';
    localStorage.removeItem('dobley.accessToken');
    localStorage.removeItem('dobley.refreshToken');
    setAuthStatus();
    state.storages = [];
    state.products = [];
    render();
}

async function loadAll() {
    if (!state.accessToken) {
        await loadProductDictionaries();
        render();
        return;
    }

    await Promise.all([loadProductDictionaries(), loadStorages()]);
    if (state.selectedStorageId) {
        await loadProducts();
    } else {
        state.products = [];
    }
    render();
}

async function loadStorages() {
    const data = await request('/api/storages?pageIndex=1&pageSize=100', {
        headers: authHeaders()
    });
    state.storages = read(data, 'collection') || [];
    if (!state.storages.some(x => read(x, 'id') === state.selectedStorageId)) {
        state.selectedStorageId = read(state.storages[0], 'id') || null;
        if (state.selectedStorageId) {
            localStorage.setItem('dobley.selectedStorageId', String(state.selectedStorageId));
        } else {
            localStorage.removeItem('dobley.selectedStorageId');
        }
    }
}

async function loadProducts() {
    const data = await request('/api/products?pageIndex=1&pageSize=100', {
        headers: authHeaders()
    });
    state.products = (read(data, 'collection') || [])
        .filter(product => !state.selectedStorageId || read(product, 'storageId') === state.selectedStorageId);
}

async function loadProductDictionaries() {
    const [loadedCategories, loadedUnitTypes] = await Promise.all([
        request('/api/products/categories'),
        request('/api/products/unit-types')
    ]);

    categories = loadedCategories || [];
    unitTypes = loadedUnitTypes || [];
    renderProductDictionaries();
}

async function saveStorage(event) {
    event.preventDefault();
    const id = byId('storage-id').value;
    const body = {
        name: byId('storage-name').value.trim(),
        description: byId('storage-description').value.trim()
    };

    const storage = await request(id ? `/api/storages/${id}` : '/api/storages/create', {
        method: id ? 'PUT' : 'POST',
        headers: authHeaders(true),
        body: JSON.stringify(body)
    });

    state.selectedStorageId = read(storage, 'id');
    localStorage.setItem('dobley.selectedStorageId', String(state.selectedStorageId));
    resetStorageForm();
    await loadAll();
    showToast('Хранилище сохранено.');
}

async function deleteStorage(id) {
    const storage = state.storages.find(x => read(x, 'id') === id);
    const storageName = read(storage, 'name') || `#${id}`;
    const confirmed = window.confirm(
        `Удалить хранилище "${storageName}"? Все продукты внутри него тоже будут удалены.`
    );

    if (!confirmed) {
        return;
    }

    await request(`/api/storages/${id}`, {
        method: 'DELETE',
        headers: authHeaders()
    });
    if (state.selectedStorageId === id) {
        state.selectedStorageId = null;
        localStorage.removeItem('dobley.selectedStorageId');
    }
    await loadAll();
    showToast('Хранилище удалено.');
}

async function saveProduct(event) {
    event.preventDefault();
    if (!state.selectedStorageId) {
        showToast('Сначала выбери хранилище.');
        return;
    }

    const id = byId('product-id').value;
    const expiration = byId('product-expiration').value;
    const body = {
        name: byId('product-name').value.trim(),
        description: byId('product-description').value.trim(),
        price: Number(byId('product-price').value),
        category: byId('product-category').value,
        unit: Number(byId('product-unit').value),
        unitType: byId('product-unit-type').value,
        barcode: byId('product-barcode').value.trim(),
        expirationDate: expiration ? `${expiration}T00:00:00Z` : null,
        storageId: state.selectedStorageId
    };

    await request(id ? `/api/products/${id}` : '/api/products/create', {
        method: id ? 'PUT' : 'POST',
        headers: authHeaders(true),
        body: JSON.stringify(body)
    });

    resetProductForm();
    await loadProducts();
    renderProducts();
    showToast('Продукт сохранен.');
}

async function deleteProduct(id) {
    await request(`/api/products/${id}`, {
        method: 'DELETE',
        headers: authHeaders()
    });
    await loadProducts();
    renderProducts();
    showToast('Продукт удален.');
}

async function openTelegramBot() {
    const bot = state.telegramBotUserName.replace('@', '').trim();
    if (!bot) {
        showToast('Username Telegram-бота не настроен в TELEGRAM_BOT_USERNAME.');
        return;
    }

    const telegramUrl = `https://t.me/${encodeURIComponent(bot)}?start=ui`;
    window.open(telegramUrl, '_blank') ?? window.location.assign(telegramUrl);

    showToast('Telegram открыт. Бот пришлет ссылку на UI.');
}

function editStorage(storage) {
    byId('storage-id').value = read(storage, 'id');
    byId('storage-name').value = read(storage, 'name');
    byId('storage-description').value = read(storage, 'description');
}

function resetStorageForm() {
    byId('storage-id').value = '';
    byId('storage-name').value = '';
    byId('storage-description').value = '';
}

function editProduct(product) {
    const expirationDate = read(product, 'expirationDate');
    byId('product-id').value = read(product, 'id');
    byId('product-name').value = read(product, 'name');
    byId('product-description').value = read(product, 'description');
    byId('product-price').value = read(product, 'price');
    byId('product-category').value = read(product, 'category');
    byId('product-unit').value = read(product, 'unit');
    byId('product-unit-type').value = read(product, 'unitType');
    byId('product-barcode').value = read(product, 'barcode');
    byId('product-expiration').value = expirationDate ? expirationDate.slice(0, 10) : '';
}

function resetProductForm() {
    byId('product-id').value = '';
    byId('product-name').value = '';
    byId('product-description').value = '';
    byId('product-price').value = '78';
    byId('product-category').value = 'Bakery';
    byId('product-unit').value = '1';
    byId('product-unit-type').value = 'Pieces';
    byId('product-barcode').value = '';
    byId('product-expiration').value = '';
}

function renderProductStorageSelect() {
    const options = state.storages.map(storage => {
        const storageId = read(storage, 'id');
        const option = document.createElement('option');
        option.value = storageId;
        option.textContent = read(storage, 'name');
        option.selected = storageId === state.selectedStorageId;
        return option;
    });

    if (options.length === 0) {
        const option = document.createElement('option');
        option.value = '';
        option.textContent = 'Нет доступных хранилищ';
        options.push(option);
    }

    elements.productStorageSelect.replaceChildren(...options);
    elements.productStorageSelect.disabled = options.length === 1 && options[0].value === '';
}

function renderProductDictionaries() {
    fillSelect(byId('product-category'), categories);
    fillSelect(byId('product-unit-type'), unitTypes);
}

function renderStorages() {
    if (state.storages.length === 0) {
        elements.storagesList.replaceChildren(createEmptyState('Хранилищ пока нет. Создай первое место хранения слева.'));
        return;
    }

    elements.storagesList.replaceChildren(...state.storages.map(storage => {
        const storageId = read(storage, 'id');
        const card = document.createElement('article');
        card.className = `item-card${storageId === state.selectedStorageId ? ' active' : ''}`;
        card.innerHTML = `
            <div class="item-main">
                <h4>${escapeHtml(read(storage, 'name'))}</h4>
                <p>${escapeHtml(read(storage, 'description'))}</p>
            </div>
            <div class="item-actions">
                <button class="secondary" data-action="select-storage" data-id="${storageId}" type="button">Выбрать</button>
                <button class="secondary" data-action="edit-storage" data-id="${storageId}" type="button">Изменить</button>
                <button class="danger" data-action="delete-storage" data-id="${storageId}" type="button">Удалить</button>
            </div>
        `;
        return card;
    }));
}

function renderProducts() {
    const storage = state.storages.find(x => read(x, 'id') === state.selectedStorageId);
    elements.selectedStorageLabel.textContent = storage
        ? `Выбрано: ${read(storage, 'name')}`
        : 'Хранилище не выбрано.';

    if (!state.selectedStorageId) {
        elements.productsList.replaceChildren(createEmptyState('Выбери хранилище, чтобы увидеть продукты.'));
        return;
    }

    if (state.products.length === 0) {
        elements.productsList.replaceChildren(createEmptyState('В этом хранилище пока нет продуктов.'));
        return;
    }

    elements.productsList.replaceChildren(...state.products.map(product => {
        const productId = read(product, 'id');
        const card = document.createElement('article');
        const expirationDate = read(product, 'expirationDate');
        const daysLeft = getDaysLeft(expirationDate);
        card.className = 'item-card';
        card.innerHTML = `
            <div class="item-main">
                <h4>${escapeHtml(read(product, 'name'))}</h4>
                <p>${escapeHtml(read(product, 'description'))}</p>
            </div>
            <div class="meta-row">
                <span class="meta">${escapeHtml(getDictionaryDisplayName(categories, read(product, 'category')))}</span>
                <span class="meta">${read(product, 'unit')} ${escapeHtml(getDictionaryDisplayName(unitTypes, read(product, 'unitType')))}</span>
                <span class="meta">${read(product, 'price')} ₽</span>
                ${expirationDate ? `<span class="meta ${daysLeft <= 3 ? 'warning' : ''}">до ${formatDate(expirationDate)}</span>` : ''}
            </div>
            <div class="item-actions">
                <button class="secondary" data-action="edit-product" data-id="${productId}" type="button">Изменить</button>
                <button class="danger" data-action="delete-product" data-id="${productId}" type="button">Удалить</button>
            </div>
        `;
        return card;
    }));
}

function createEmptyState(message) {
    const emptyState = document.createElement('div');
    emptyState.className = 'empty-state';
    emptyState.textContent = message;
    return emptyState;
}

function getDictionaryDisplayName(dictionary, name) {
    const item = dictionary.find(x => read(x, 'name') === name);

    return read(item, 'displayName') || name;
}

function render() {
    setAuthStatus();
    renderTabs();
    renderProductStorageSelect();
    renderStorages();
    renderProducts();
}

function formatDate(value) {
    return new Intl.DateTimeFormat('ru-RU').format(new Date(value));
}

function getDaysLeft(value) {
    if (!value) {
        return Number.POSITIVE_INFINITY;
    }
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const expiration = new Date(value);
    expiration.setHours(0, 0, 0, 0);
    return Math.round((expiration - today) / 86400000);
}

function escapeHtml(value) {
    return String(value ?? '')
        .replaceAll('&', '&amp;')
        .replaceAll('<', '&lt;')
        .replaceAll('>', '&gt;')
        .replaceAll('"', '&quot;')
        .replaceAll("'", '&#039;');
}

function read(source, name) {
    if (!source) {
        return undefined;
    }

    return source[name] ?? source[name[0].toUpperCase() + name.slice(1)];
}

function wireEvents() {
    byId('login-button').addEventListener('click', () => run(login));
    byId('register-button').addEventListener('click', () => run(register));
    byId('logout-button').addEventListener('click', logout);
    byId('reload-button').addEventListener('click', () => run(loadAll));
    elements.storagesTabButton.addEventListener('click', () => setActiveTab('storages'));
    elements.productsTabButton.addEventListener('click', () => setActiveTab('products'));
    elements.productStorageSelect.addEventListener('change', () => run(async () => {
        state.selectedStorageId = Number(elements.productStorageSelect.value) || null;
        if (state.selectedStorageId) {
            localStorage.setItem('dobley.selectedStorageId', String(state.selectedStorageId));
        } else {
            localStorage.removeItem('dobley.selectedStorageId');
        }

        await loadProducts();
        render();
    }));
    byId('open-telegram-button').addEventListener('click', () => run(openTelegramBot));
    byId('storage-form').addEventListener('submit', event => run(() => saveStorage(event)));
    byId('product-form').addEventListener('submit', event => run(() => saveProduct(event)));
    byId('reset-storage-button').addEventListener('click', resetStorageForm);
    byId('reset-product-button').addEventListener('click', resetProductForm);

    elements.storagesList.addEventListener('click', event => run(async () => {
        const button = event.target.closest('button[data-action]');
        if (!button) {
            return;
        }
        const id = Number(button.dataset.id);
        const storage = state.storages.find(x => read(x, 'id') === id);
        if (button.dataset.action === 'select-storage') {
            state.selectedStorageId = id;
            localStorage.setItem('dobley.selectedStorageId', String(id));
            setActiveTab('products');
            await loadProducts();
            render();
        }
        if (button.dataset.action === 'edit-storage' && storage) {
            editStorage(storage);
        }
        if (button.dataset.action === 'delete-storage') {
            await deleteStorage(id);
        }
    }));

    elements.productsList.addEventListener('click', event => run(async () => {
        const button = event.target.closest('button[data-action]');
        if (!button) {
            return;
        }
        const id = Number(button.dataset.id);
        const product = state.products.find(x => read(x, 'id') === id);
        if (button.dataset.action === 'edit-product' && product) {
            editProduct(product);
        }
        if (button.dataset.action === 'delete-product') {
            await deleteProduct(id);
        }
    }));
}

async function run(action) {
    try {
        await action();
    } catch (error) {
        showToast(error.message);
    }
}

renderProductDictionaries();
wireEvents();
render();
run(loadAll);
