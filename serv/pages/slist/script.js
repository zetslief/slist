async function addNewItem(button) {
    const input = button.previousElementSibling;
    const item = newItemDto(input.value);
    const response = await fetch("/items/new", {
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

function newItemDto(content) {
    return { content: content };
}