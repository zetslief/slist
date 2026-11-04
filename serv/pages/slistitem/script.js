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

async function renameItem(button) { 
    const item = button.closest(".list-item");
    item.classList.add("-edit");
    const label = item.querySelector(".-list-item-title-marker");
    const renameInput = item.querySelector(".-list-item-rename-value-marker");
    renameInput.value = label.textContent;
}