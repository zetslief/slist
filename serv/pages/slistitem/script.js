async function toggleItem(input) {
    const itemId = input.id;
    const response = await fetch(`/items/${itemId}/toggle`, {
        method: 'POST'
    });
    if (!response.ok) {
        console.error(response);
        return;
    }
    const result = await response.json();
    input.checked = result.checked; 
    console.log(result);
}

async function deleteItem(button) {
    const item = button.closest(".list-item");
    const input = item.querySelector(".-list-item-value-marker");
    const itemId = input.id;
    const response = await fetch (`/items/${itemId}`, {
        method: "DELETE",
    });
    if (!response.ok) {
        console.error(response);
        return;
    }
    item.remove();
}

async function copyItem(button) {
    const item = button.closest(".list-item");
    const label = item.querySelector(".-list-item-title-marker");
    console.log(`Copy: '${label.textContent}'`);
    await navigator.clipboard.writeText(label.textContent);
}

function getItemLabelAndRenameInput(item) {
    const label = item.querySelector(".-list-item-title-marker");
    const renameInput = item.querySelector(".-list-item-rename-value-marker");
    return { label, renameInput };
}

function startRenameItem(button) { 
    const item = button.closest(".list-item");
    const { label, renameInput } = getItemLabelAndRenameInput(item);
    renameInput.value = label.textContent;
    item.classList.add("-edit");
    renameInput.focus();
}

function acceptRenameItem(button) {
    const item = button.closest(".list-item");
    const { label, renameInput } = getItemLabelAndRenameInput(item);
    label.textContent = renameInput.value;
    item.classList.remove("-edit");
}