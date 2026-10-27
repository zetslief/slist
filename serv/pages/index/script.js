const newListNameInput = document.getElementById("new-list-name");
const getNewListName = () => newListNameInput.value;

async function createList() {
  const title = getNewListName();
  var response = await fetch("/lists/new", {
    method: 'POST',
    headers: { 'Content-Type' : 'application/json' },
    body: JSON.stringify(newListDto(title)),
  })
  if (!response.ok) {
    console.error(response);
    return;
  }
  console.log(await response.json());
}

function newListDto(title) {
  return { title: title };
}