const state = {
    accessToken: localStorage.getItem('dobley.accessToken') || '',
    refreshToken: localStorage.getItem('dobley.refreshToken') || '',
    selectedStorageId: Number(localStorage.getItem('dobley.selectedStorageId')) || null,
    storages: [],
    products: [],
    recipients: [],
    telegramBotUserName: localStorage.getItem('dobley.telegramBotUserName')
        || window.DobleyUiConfig?.telegramBotUserName
        || ''
};

const categories = [
    'Dairy', 'Cheese', 'Eggs', 'RawMeat', 'FishAndSeafood', 'DeliAndSausages',
    'Vegetables', 'FruitsAndBerries', 'HerbsAndGreens', 'Beverages',
    'SaucesAndCondiments', 'ReadyMealsAndLeftovers', 'Bakery',
    'OpenedCannedGoods', 'BabyFood', 'NonFood'
];

const unitTypes = [
    'Grams', 'Kilograms', 'Milligrams', 'Milliliters', 'Liters', 'Pieces',
    'Servings', 'Packs', 'Jars', 'Bottles', 'Centimeters'
];

const byId = id => document.getElementById(id);

const elements = {
    authStatus: byId('auth-status'),
    login: byId('login'),
    password: byId('password'),
    telegramBot: byId('telegram-bot'),
    recipientSelect: byId('recipient-select'),
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
        return response.json();
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
    elements.authStatus.textContent = state.accessToken ? 'online' : 'offline';
    elements.authStatus.classList.toggle('online', Boolean(state.accessToken));
}

function fillSelect(select, values) {
    select.replaceChildren(...values.map(value => {
        const option = document.createElement('option');
        option.value = value;
        option.textContent = value;
        return option;
    }));
}

async function login() {
    const body = {
        login: elements.login.value.trim(),
        password: elements.password.value
    };

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
    showToast('Вход выполнен.');
}

function logout() {
    state.accessToken = '';
    state.refreshToken = '';
    localStorage.removeItem('dobley.accessToken');
    localStorage.removeItem('dobley.refreshToken');
    setAuthStatus();
    state.storages = [];
    state.products = [];
    state.recipients = [];
    render();
}

async function loadAll() {
    if (!state.accessToken) {
        render();
        return;
    }

    await Promise.all([loadStorages(), loadRecipients()]);
    if (state.selectedStorageId) {
        await loadProducts();
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
    }
}

async function loadProducts() {
    const data = await request('/api/products?pageIndex=1&pageSize=100', {
        headers: authHeaders()
    });
    state.products = (read(data, 'collection') || [])
        .filter(product => !state.selectedStorageId || read(product, 'storageId') === state.selectedStorageId);
}

async function loadRecipients() {
    state.recipients = await request('/api/notifications/recipients', {
        headers: authHeaders()
    });
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

async function createInviteAndOpenTelegram() {
    const invite = await request('/api/notifications/invites/create', {
        method: 'POST',
        headers: authHeaders(true),
        body: JSON.stringify({ expiresAt: null })
    });

    const bot = state.telegramBotUserName.replace('@', '').trim();
    if (!bot) {
        showToast(`Код создан: ${read(invite, 'code')}. Укажи username бота, чтобы открыть Telegram ссылкой.`);
        return;
    }

    const code = read(invite, 'code');
    window.open(`https://t.me/${encodeURIComponent(bot)}?start=${encodeURIComponent(code)}`, '_blank');
    showToast(`Код ${code} создан. Telegram открыт.`);
}

async function subscribeSelectedRecipient() {
    const recipientId = Number(elements.recipientSelect.value);
    if (!recipientId) {
        showToast('Сначала подключи Telegram-чат через код.');
        return;
    }

    const storageIds = state.storages.map(storage => read(storage, 'id'));
    if (storageIds.length === 0) {
        showToast('Сначала создай хотя бы одно хранилище.');
        return;
    }

    await request(`/api/notifications/recipients/${recipientId}/subscriptions`, {
        method: 'POST',
        headers: authHeaders(true),
        body: JSON.stringify({
            storageIds,
            notifyBeforeDays: 3
        })
    });

    showToast('Рассылка для чата включена.');
}

async function unsubscribeSelectedRecipient() {
    const recipientId = Number(elements.recipientSelect.value);
    if (!recipientId) {
        showToast('Сначала выбери подключенный чат.');
        return;
    }

    await request(`/api/notifications/recipients/${recipientId}/subscriptions`, {
        method: 'DELETE',
        headers: authHeaders()
    });

    showToast('Рассылка для чата выключена.');
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

function renderRecipients() {
    const options = state.recipients.map(recipient => {
        const option = document.createElement('option');
        option.value = read(recipient, 'id');
        option.textContent = `${read(recipient, 'displayName') || read(recipient, 'externalId')} (${read(recipient, 'channel')})`;
        return option;
    });

    if (options.length === 0) {
        const option = document.createElement('option');
        option.value = '';
        option.textContent = 'Нет подключенных чатов';
        options.push(option);
    }

    elements.recipientSelect.replaceChildren(...options);
}

function renderStorages() {
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
                <span class="meta">${escapeHtml(read(product, 'category'))}</span>
                <span class="meta">${read(product, 'unit')} ${escapeHtml(read(product, 'unitType'))}</span>
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

function render() {
    setAuthStatus();
    elements.telegramBot.value = state.telegramBotUserName;
    renderRecipients();
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
    byId('logout-button').addEventListener('click', logout);
    byId('reload-button').addEventListener('click', () => run(loadAll));
    byId('refresh-recipients-button').addEventListener('click', () => run(async () => {
        await loadRecipients();
        renderRecipients();
    }));
    byId('save-telegram-button').addEventListener('click', () => {
        state.telegramBotUserName = elements.telegramBot.value.trim().replace('@', '');
        localStorage.setItem('dobley.telegramBotUserName', state.telegramBotUserName);
        showToast('Telegram username сохранен.');
    });
    byId('open-telegram-button').addEventListener('click', () => run(createInviteAndOpenTelegram));
    byId('subscribe-recipient-button').addEventListener('click', () => run(subscribeSelectedRecipient));
    byId('unsubscribe-recipient-button').addEventListener('click', () => run(unsubscribeSelectedRecipient));
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

fillSelect(byId('product-category'), categories);
fillSelect(byId('product-unit-type'), unitTypes);
wireEvents();
render();
run(loadAll);
