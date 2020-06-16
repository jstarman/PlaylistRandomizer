const uri = 'spotify';

function getAuthorization() {
    fetch(`${uri}/authorize`, {
        credentials: 'include',
        redirect: 'follow',
        mode: "no-cors"
    })
        .then(response => response.json())
        .then(data => displayUser(data))
        .catch(error => console.error('Authorization failed.', error));
}

function displayUser(data) {
    document.getElementById('username').innerText = `Authorized as ${data.display_name}`;
    getPlayLists();
}

function getPlayLists() {
    fetch(`${uri}/playlists`)
        .then(response => response.json())
        .then(data => {
            displayPlaylists(data);
        })
        .catch(error => console.error('Unable to load playlists.', error));
}

function displayPlaylists(data) {

    const tBody = document.getElementById('playlists');
    tBody.innerHTML = '';

    const button = document.createElement('button');

    data.forEach(item => {
        let shuffleButton = button.cloneNode(false);
        shuffleButton.innerText = 'Shuffle';
        shuffleButton.setAttribute('onclick', `shuffleList('${item.id}')`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        td1.appendChild(shuffleButton);

        let td2 = tr.insertCell(1);
        let textNode = document.createTextNode(item.name);
        td2.appendChild(textNode);
    });
}

function shuffleList(id) {
    fetch(`${uri}/shuffle/${id}`, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        }
    })
        .then(() => getPlayLists())
        .catch(error => console.error('Unable to shuffle playlists.', error));
}
