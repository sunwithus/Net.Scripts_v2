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


////////////////////////////////////////////////////
//let wavesurfer;
//Single audio WaveForm
window.createAudioSingletrack = async (url, FileName, AutoPlay) => {

    //Initialize the Regions plugin
    const regions = WaveSurfer.Regions.create();

    const wavesurfer = WaveSurfer.create({
        container: '#playercontainer',
        waveColor: '#4F4A85',
        progressColor: '#181331',
        url: url,
        height: 60,
        splitChannels: true,
        normalize: false,
        cursorColor: "#ddd5e9",
        cursorWidth: 2,
        barWidth: 2,
        barGap: 1,
        barRadius: 10,
        barHeight: 1.1,
        barAlign: "",
        minPxPerSec: 10,
        fillParent: true,
        mediaControls: true,
        autoplay: AutoPlay,
        interact: true,
        dragToSeek: false,
        hideScrollbar: true,
        audioRate: 1,
        autoScroll: true,
        autoCenter: true,
        //autoScroll: false, // Disable auto scroll
        sampleRate: 8000,
        plugins: [regions],
    })

    const zoomLevel = document.getElementById('zoomlevel');
    const speedLevel = document.getElementById('speedlevel');
    const currentSpeed = document.getElementById('currentspeed');
    const loopRegions = document.getElementById('loopregions');
    const forwardButton = document.querySelector('#forwardbutton')
    const backwardButton = document.querySelector('#backwardbutton')

    // Forward/back buttons
    forwardButton.onclick = () => {
        wavesurfer.setTime(wavesurfer.getCurrentTime() + 5)
    }
    backwardButton.onclick = () => {
        wavesurfer.setTime(wavesurfer.getCurrentTime() - 5)
    }

    wavesurfer.on('play', () => {
        console.log('Play');
    })
    /** When the audio pauses */
    wavesurfer.on('pause', () => {
        console.log('Pause');
    })
    /** When the audio finishes playing */
    wavesurfer.on('finish', () => {
        console.log('Finish');
    })
    /** Just before the waveform is destroyed so you can clean up your events */
    wavesurfer.on('destroy', () => {
        console.log('Destroy')
    })
    /** When the audio is both decoded and can play */
    wavesurfer.on('ready', (duration) => {
        console.log('Ready', duration + 's')
    })
    // Update the zoom level on slider change
    wavesurfer.once('decode', () => {
        zoomLevel.addEventListener('input', (e) => {
            const minPxPerSec = e.target.valueAsNumber
            wavesurfer.zoom(minPxPerSec)
        })

    })
    // Set the playback rate
    const speeds = [0.5, 0.7, 0.85, 1, 1.15, 1.3, 1.5]
    speedLevel.addEventListener('input', (e) => {
        const speed = speeds[e.target.valueAsNumber]
        currentSpeed.textContent = speed.toFixed(2) // количество знаков после запятой
        wavesurfer.setPlaybackRate(speed, 'true') // true = preserve Pitch (сохранение высоты тона)
    })

    ///////////////////////////////////////////////////////////////////
    // regions

    const selectedColor = 'rgba(255, 255, 0, 0.2)';
    const defaultColor = 'rgba(255, 255, 0, 0.1)';

    regions.enableDragSelection({
        color: defaultColor,
    })
    //When the region's parameters are being updated (example:move event)
    regions.on('region-updated', (region) => {
        console.log('Updated region', region) 
    })
    // Toggle looping with a checkbox
    let loop = true;
    loopRegions.onclick = (e) => {
        loop = e.target.checked
    }
    // Add event listener for right click to remove the region
    regions.on('region-double-clicked', (region, e) => {
        e.stopPropagation(); // Prevent triggering a click on the waveform
        e.preventDefault(); // Prevent default behavior
        activeRegion = region
        region.remove();
        activeRegion = null
        console.log('Region removed', region);
    })  
    {
        let activeRegion = null
        regions.on('region-in', (region) => {
            console.log('region-in', region)
            activeRegion = region
            region.setOptions({ color: selectedColor })

        })
        regions.on('region-out', (region) => {
            console.log('region-out', region)
            if (activeRegion === region) {
                if (loop) {
                    region.play()
                } else {
                    region.setOptions({ color: defaultColor })
                    activeRegion = null
                }
            }
        })
        regions.on('region-clicked', (region, e) => {
            e.stopPropagation(); // Prevent triggering a click on the waveform
            e.preventDefault(); // Prevent default behavior
            activeRegion = region
            region.play()
        })
        // Reset the active region when the user clicks anywhere in the waveform
        wavesurfer.on('interaction', () => {
            activeRegion = null
        })
    }

    /////////////////////////////////////////////////
    const saveAudioButton = document.getElementById('saveaudio');
    if (saveAudioButton) {
        saveAudioButton.addEventListener('click', function () {
            if (!url) {
                console.error('Аудио URL отсутствует.');
                return;
            }

            // Создаём ссылку для скачивания
            const downloadLink = document.createElement('a');
            downloadLink.href = url;
            downloadLink.download = FileName; // Указываем имя файла
            document.body.appendChild(downloadLink); // Добавляем ссылку в DOM
            downloadLink.click(); // Инициируем скачивание
            document.body.removeChild(downloadLink); // Удаляем ссылку
        });
    }
};
/*

window.stopAudio = () => {
    if (wavesurfer) {
        wavesurfer.pause();
        wavesurfer.setTime(0);
    } else {
        console.error('wavesurfer is not initialized');
    }
};
window.playAudio = () => {
    if (wavesurfer) {
        wavesurfer.play();
    } else {
        console.error('wavesurfer is not initialized');
    }
};
*/

/*
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
                    trackBackground: '#FFFFFF',
                    height: 70,
                    splitChannels: true,
                    normalize: false,
                    cursorColor: "#ddd5e9",
                    cursorWidth: 2,
                    barWidth: 2,
                    barGap: 1,
                    barRadius: 10,
                    barHeight: 1.1,
                    barAlign: "",
                    minPxPerSec: 64,
                    fillParent: true,
                    mediaControls: true,
                    autoplay: false,
                    interact: true,
                    dragToSeek: false,
                    hideScrollbar: true,
                    audioRate: 1,
                    autoScroll: true,
                    autoCenter: true,
                    sampleRate: 8000,
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
                    height: 70,
                },
            },
        ],
        {
            container: document.querySelector('#playercontainer'), // required!
            minPxPerSec: 10, // zoom level
            rightButtonDrag: false, // set to true to drag with right mouse button
            cursorWidth: 2,
            cursorColor: '#D72F21',
            trackBackground: '#FFFFFF',
            trackBorderColor: '#7C7C7C',
            dragBounds: false,
            waveHeight: 0, // высота шкалы времени

        },
    )

    // Play/pause button
    const playpauseButton = document.querySelector('#playbutton')
    playpauseButton.disabled = true
    multitrack.once('canplay', () => {
        playpauseButton.disabled = false
        playpauseButton.onclick = () => {
            multitrack.isPlaying() ? multitrack.pause() : multitrack.play()
            playpauseButton.textContent = multitrack.isPlaying() ? '⏸' : '▶'
        }
    })

    // Stop button
    const stopButton = document.querySelector('#stopbutton')
    stopButton.onclick = () => {
        multitrack.pause();
        multitrack.setTime(0); // Optionally, reset the time to 0 after stopping
        console.log('multitrack.stop()')
    };

    // Forward/back buttons
    const forwardButton = document.querySelector('#forwardbutton')
    forwardButton.onclick = () => {
        multitrack.setTime(multitrack.getCurrentTime() + 10)
    }
    const backwardButton = document.querySelector('#backwardbutton')
    backwardButton.onclick = () => {
        multitrack.setTime(multitrack.getCurrentTime() - 10)
    }

    // Volume sliders for left and right tracks
    const volumeSliderLeft = document.getElementById('volumeslider-left');
    const volumeSliderRight = document.getElementById('volumeslider-right');


    multitrack.on('volume-change', ({ id, volume }) => {
        console.log(`Track ${id} volume updated to ${volume}`)
    })
    volumeSliderLeft.addEventListener('input', function () {
        multitrack.setVolume(0, this.value / 100); // Adjust volume for the left track
        console.log('volumeSliderLeft: ' + this.value)
    });
    volumeSliderRight.addEventListener('input', function () {
        multitrack.setVolume(1, this.value / 100); // Adjust volume for the right track
        console.log('volumeSliderRight: ' + this.value)
    });


    // Destroy all wavesurfer instances on unmount
    // This should be called before calling initMultiTrack again to properly clean up
    window.onbeforeunload = () => {
        multitrack.destroy()
    }
  
};
*/

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
