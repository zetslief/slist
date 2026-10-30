const itemTemplate = document.getElementById("list-item-template");

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
    const createdItem = await response.json();
    const listItems = list.querySelector(".list-items");
    const newNode = createItemNode(createdItem);
    listItems.appendChild(newNode);
    input.value = "";
}

function createItemNode(item) {
    const itemFragment = itemTemplate.content.cloneNode(true);
    const input = itemFragment.querySelector("input");
    const label = itemFragment.querySelector("label");
    input.id = item.id;
    label.htmlFor = item.id;
    label.textContent = item.name;
    return itemFragment;
}

function newItemDto(listId, content) {
    return { listId: listId, name: content };
}