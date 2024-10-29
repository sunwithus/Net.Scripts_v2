// показать / скрыть элемент по id
toggleElement = (id) => {
    let element = document.getElementById(id);
    console.log("Element is " + id + ": " + element.style.visibility);
    if (element.style.visibility = "hidden") {
        element.style.visibility = "visible";
    }
    else {
        element.style.visibility = "hidden";
    }
    console.log("Element is " + id + ": " + element.style.visibility);
}

showElement = (id) => {
    let element = document.getElementById(id);
    element.style.visibility = "visible";
    console.log("showElement " + id);
}

hideElement = (id) => {
    let element = document.getElementById(id);
    element.style.visibility = "hidden";
    console.log("hideElement " + id);
}

clearTextElement = (id) => {
    let element = document.getElementById(id);
    element.textContent = "";
    console.log("textContent of " + id + ": " + element.style.visibility);
}

window.saveAsFile = (data, filename) => {
    const blob = new Blob([data], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
}

window.saveExcelAsFile = (data, fileName) => {
    const blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    const url = window.URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName;
    anchorElement.click();
    anchorElement.remove();
    window.URL.revokeObjectURL(url);
}

function downloadFile(filename, byteBase64) {
    var link = document.createElement('a');
    link.href = "data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64," + byteBase64;
    link.download = filename;
    link.click();
}

window.PlayAudioFileStream = async (contentStreamReference, id) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    var sound = document.getElementById(id);
    if (!sound) {
        sound = document.createElement('audio');
        sound.id = id;
        document.body.appendChild(sound);
    }
    sound.src = url;
    sound.type = 'audio/mpeg'; // Adjust the type based on your audio format
    sound.load();
};

window.PlayAudio = (id) => {
    var sound = document.getElementById(id);
    sound.play();
};

window.PauseAudio = (id) => {
    var sound = document.getElementById(id);
    sound.pause();
};

window.StopAudio = (id) => {
    var sound = document.getElementById(id);
    sound.pause();
    sound.currentTime = 0;
};

