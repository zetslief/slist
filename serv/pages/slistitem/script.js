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