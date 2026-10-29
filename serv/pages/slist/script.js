async function addNewItem(button) {
    const input = button.previousElementSibling;
    const list = button.closest(".list");
    const item = newItemDto(list.id, input.value);
    const response = await fetch(`/items/new`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(item),
    });
    if (!response.ok) {
        console.error(response);
        return;
    }
    console.log(await response.json());
}

function newItemDto(listId, content) {
    return { listId: listId, name: content };
}