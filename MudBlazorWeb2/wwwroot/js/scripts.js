// script.js
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
/* Audio Standart HTML
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
*/

//Single audio WaveForm
window.createAudioBlob = async (audioData, id) => {
    const arrayBuffer = await audioData.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    const wavesurfer = WaveSurfer.create({
        container: id,
        waveColor: '#4F4A85',
        progressColor: '#383351',
        url: url,
    });
    document.getElementById('playButton').addEventListener('click', function () {
        wavesurfer.playPause();
    });

    document.getElementById('stopButton').addEventListener('click', function () {
        wavesurfer.stop();
    });

    document.getElementById('volumeSlider').addEventListener('input', function () {
        wavesurfer.setVolume(this.value / 100);
    });
};

//Multitrack Audio WaveForm
window.createAudioMultitrack = async (audioDataLeft, audioDataRight) => {

    const arrayBufferL = await audioDataLeft.arrayBuffer();
    const arrayBufferR = await audioDataRight.arrayBuffer();
    const blobL = new Blob([arrayBufferL]);
    const blobR = new Blob([arrayBufferR]);
    const urlL = URL.createObjectURL(blobL);
    const urlR = URL.createObjectURL(blobR);

    const multitrack = Multitrack.create(
        [
            {
                id: 0,
                draggable: false,
                startPosition: 0,
                volume: 0.8,
                url: urlL,
                options: {
                    waveColor: '#4F4A85',
                    progressColor: '#383351',
                    height: 80,
                },
            },
            {
                id: 1,
                draggable: false,
                startPosition: 0,
                volume: 0.8,
                url: urlR,
                options: {
                    waveColor: '#4F4A85',
                    progressColor: '#383351',
                    height: 80,
                },
            },
        ],
        {
            container: document.querySelector('#playercontainer'), // required!
            
            rightButtonDrag: false, // set to true to drag with right mouse button
            cursorWidth: 2,
            cursorColor: '#D72F21',
            trackBackground: '#FFFFFF',
            trackBorderColor: '#7C7C7C',
            dragBounds: true,

        },
    )

    // Play/pause button
    const button = document.querySelector('#playbutton')
    button.disabled = true
    multitrack.once('canplay', () => {
        button.disabled = false
        button.onclick = () => {
            multitrack.isPlaying() ? multitrack.pause() : multitrack.play()
            button.textContent = multitrack.isPlaying() ? '⏸' : '▶'
        }
    })

    // Forward/back buttons
    const forward = document.querySelector('#forwardbutton')
    forward.onclick = () => {
        multitrack.setTime(multitrack.getCurrentTime() + 10)
    }
    const backward = document.querySelector('#backwardbutton')
    backward.onclick = () => {
        multitrack.setTime(multitrack.getCurrentTime() - 10)
    }
    document.getElementById('volumeslider').addEventListener('input', function () {
        multitrack.setVolume(this.value / 100);
    });

    // Destroy all wavesurfer instances on unmount
    // This should be called before calling initMultiTrack again to properly clean up
    window.onbeforeunload = () => {
        multitrack.destroy()
    }

    // Set sinkId
    multitrack.once('canplay', async () => {
        await multitrack.setSinkId('default')
        console.log('Set sinkId to default')
    })
};