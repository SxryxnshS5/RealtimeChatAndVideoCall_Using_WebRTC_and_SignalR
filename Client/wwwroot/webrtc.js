let localStream;
let remoteStream;
let peerConnection;
const iceServers = { iceServers: [{ urls: 'stun:stun.l.google.com:19302' }] };

async function startVideoCall(signalHandler) {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        document.getElementById('localVideo').srcObject = localStream;

        peerConnection = new RTCPeerConnection(iceServers);
        localStream.getTracks().forEach(track => {
            console.log("Adding track:", track);
            peerConnection.addTrack(track, localStream);
        });

        peerConnection.onicecandidate = event => {
            if (event.candidate) {
                console.log("Sending ICE Candidate:", event.candidate);
                signalHandler.invokeMethodAsync('SendIceCandidate', JSON.stringify(event.candidate));
            }
        };

        peerConnection.ontrack = event => {
            console.log("Track received:", event.track.kind);
            if (!remoteStream) {
                remoteStream = new MediaStream();
                document.getElementById('remoteVideo').srcObject = remoteStream;
            }
            remoteStream.addTrack(event.track);
        };
    } catch (err) {
        console.error("Error starting video call:", err);
    }
}

async function createOffer(signalHandler) {
    try {
        const offer = await peerConnection.createOffer();
        console.log("Creating offer:", offer);
        await peerConnection.setLocalDescription(offer);
        signalHandler.invokeMethodAsync('SendOffer', JSON.stringify(offer));
    } catch (err) {
        console.error("Error creating offer:", err);
    }
}

async function handleOffer(offer, signalHandler) {
    try {
        const offerDescription = new RTCSessionDescription(JSON.parse(offer));
        await peerConnection.setRemoteDescription(offerDescription);

        const answer = await peerConnection.createAnswer();
        await peerConnection.setLocalDescription(answer);
        signalHandler.invokeMethodAsync('SendAnswer', JSON.stringify(answer));
    } catch (err) {
        console.error("Error handling offer:", err);
    }
}

async function handleAnswer(answer) {
    try {
        const answerDescription = new RTCSessionDescription(JSON.parse(answer));
        await peerConnection.setRemoteDescription(answerDescription);
    } catch (err) {
        console.error("Error handling answer:", err);
    }
}

async function addIceCandidate(candidate) {
    try {
        const iceCandidate = new RTCIceCandidate(JSON.parse(candidate));
        await peerConnection.addIceCandidate(iceCandidate);
    } catch (err) {
        console.error("Error adding ICE candidate:", err);
    }
}

function endCall() {
    if (peerConnection) {
        peerConnection.close();
        peerConnection = null;
    }
    if (localStream) {
        localStream.getTracks().forEach(track => track.stop());
    }
    remoteStream = null;
    document.getElementById('localVideo').srcObject = null;
    document.getElementById('remoteVideo').srcObject = null;
}
