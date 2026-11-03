const itemTemplate = document.getElementById("list-item-template");

async function addNewItem(event) {
    event.preventDefault();
    const form = event.target;
    const list = form.closest(".list");
    const input = form.querySelector(".-new-item-marker");
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
    input.focus();
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

async function copyListToClipboard(button) {
    const list = button.closest(".list");
    const title = list.querySelector(".-title-marker");
    const items = list.querySelectorAll(".list-item");
    let result = `# ${title.textContent}:`
    for (const item of items) {
        const input = item.querySelector(".-list-item-value-marker"); 
        const label = item.querySelector(".-list-item-title-marker"); 
        result += `\n- [${(input.checked ? 'x' : ' ')}] ${label.textContent}`;
    }
    console.log(result);
    await navigator.clipboard.writeText(result);
}

async function shareListToClipboard(button) {
    const list = button.closest(".list");
    const link = `${location.href}lists/${list.id}`;
    console.log(link);
    await navigator.clipboard.writeText(link);
}

function newItemDto(listId, content) {
    return { listId: listId, name: content };
}