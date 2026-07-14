document.addEventListener("DOMContentLoaded", () => {
    const pageElement = document.querySelector(".chat-page[data-room-id]");
    const statusElement = document.getElementById("chatStatus");
    const messagesElement = document.getElementById("chatMessages");
    const formElement = document.getElementById("chatForm");
    const inputElement = document.getElementById("chatMessageInput");
    const sendButton = formElement?.querySelector(".chat-send-button");
    const roomId = Number(pageElement?.dataset.roomId);
    const currentUserId = Number(pageElement?.dataset.currentUserId);
    const recordSeparator = String.fromCharCode(0x1e);

    let socket = null;
    let invocationId = 0;
    let isRoomClosed = false;

    if (!roomId || !statusElement || !messagesElement || !formElement || !inputElement) {
        return;
    }

    const setStatus = (text, className) => {
        statusElement.textContent = text;
        statusElement.classList.remove("is-connected", "is-disconnected");

        if (className) {
            statusElement.classList.add(className);
        }
    };

    const setFormEnabled = (enabled) => {
        inputElement.disabled = !enabled || isRoomClosed;

        if (sendButton) {
            sendButton.disabled = !enabled || isRoomClosed;
        }
    };

    const appendSystemMessage = (message) => {
        messagesElement.querySelector(".chat-empty")?.remove();

        const item = document.createElement("div");
        item.className = "chat-system-message";
        item.textContent = message;

        messagesElement.appendChild(item);
        messagesElement.scrollTop = messagesElement.scrollHeight;
    };

    const closeRoom = (message) => {
        if (isRoomClosed) {
            return;
        }

        isRoomClosed = true;
        setStatus("채팅방 종료", "is-disconnected");
        setFormEnabled(false);
        appendSystemMessage(message || "채팅방이 종료되었습니다.");
    };

    const updateReadState = (messageIds) => {
        messageIds.forEach((messageId) => {
            const messageElement = messagesElement.querySelector(`.chat-message.mine[data-message-id="${messageId}"]`);
            const readStateElement = messageElement?.querySelector(".chat-read-state");

            if (readStateElement) {
                readStateElement.textContent = "읽음";
            }
        });
    };

    const appendMessage = (senderName, message, sentAt, messageId, senderId) => {
        messagesElement.querySelector(".chat-empty")?.remove();

        const isMine = senderId === currentUserId;
        const item = document.createElement("article");
        item.className = `chat-message ${isMine ? "mine" : "theirs"}`;
        item.dataset.messageId = messageId;
        item.dataset.senderId = senderId;

        const meta = document.createElement("div");
        meta.className = "chat-message-meta";

        const sender = document.createElement("strong");
        sender.textContent = senderName;

        const time = document.createElement("span");
        time.textContent = sentAt;

        const content = document.createElement("div");
        content.className = "chat-message-content";
        content.textContent = message;

        meta.append(sender, time);
        item.append(meta, content);

        if (isMine) {
            const readState = document.createElement("div");
            readState.className = "chat-read-state";
            readState.textContent = "안읽음";
            item.appendChild(readState);
        }

        messagesElement.appendChild(item);
        messagesElement.scrollTop = messagesElement.scrollHeight;
    };

    const sendHubMessage = (target, args) => {
        if (!socket || socket.readyState !== WebSocket.OPEN) {
            throw new Error("WebSocket is not connected.");
        }

        if (isRoomClosed) {
            return;
        }

        const payload = {
            type: 1,
            invocationId: String(++invocationId),
            target,
            arguments: args
        };

        socket.send(JSON.stringify(payload) + recordSeparator);
    };

    const joinRoom = () => {
        sendHubMessage("JoinRoom", [roomId]);
    };

    const markRoomAsRead = () => {
        sendHubMessage("MarkRoomAsRead", [roomId]);
    };

    const handleHubPayload = (payload) => {
        if (!payload || payload.type !== 1) {
            return;
        }

        if (payload.target === "ReceiveMessage") {
            const [senderName, message, sentAt, messageId, senderId] = payload.arguments ?? [];
            appendMessage(senderName ?? "", message ?? "", sentAt ?? "", messageId, senderId);

            if (senderId !== currentUserId) {
                markRoomAsRead();
            }

            return;
        }

        if (payload.target === "MessagesRead") {
            const [, messageIds] = payload.arguments ?? [];
            updateReadState(messageIds ?? []);
            return;
        }

        if (payload.target === "RoomClosed") {
            const [message] = payload.arguments ?? [];
            closeRoom(message);
        }
    };

    const handleSocketMessage = (event) => {
        const messages = String(event.data)
            .split(recordSeparator)
            .filter(Boolean);

        messages.forEach((rawMessage) => {
            try {
                handleHubPayload(JSON.parse(rawMessage));
            } catch (error) {
                console.error(error);
            }
        });
    };

    const connect = async () => {
        setStatus("연결 중", "is-disconnected");
        setFormEnabled(false);

        const negotiateResponse = await fetch("/chatHub/negotiate?negotiateVersion=1", {
            method: "POST",
            credentials: "same-origin"
        });

        if (!negotiateResponse.ok) {
            throw new Error("SignalR negotiate request failed.");
        }

        const negotiateData = await negotiateResponse.json();
        const connectionToken = encodeURIComponent(negotiateData.connectionToken);
        const protocol = window.location.protocol === "https:" ? "wss" : "ws";
        const socketUrl = `${protocol}://${window.location.host}/chatHub?id=${connectionToken}`;

        socket = new WebSocket(socketUrl);

        socket.onopen = () => {
            socket.send(JSON.stringify({ protocol: "json", version: 1 }) + recordSeparator);
            joinRoom();
            setStatus("연결됨", "is-connected");
            setFormEnabled(true);
            messagesElement.scrollTop = messagesElement.scrollHeight;
            inputElement.focus();
        };

        socket.onmessage = handleSocketMessage;

        socket.onerror = (event) => {
            console.error(event);
            setStatus("연결 오류", "is-disconnected");
            setFormEnabled(false);
        };

        socket.onclose = () => {
            if (isRoomClosed) {
                return;
            }

            setStatus("연결 끊김", "is-disconnected");
            setFormEnabled(false);
        };
    };

    formElement.addEventListener("submit", (event) => {
        event.preventDefault();

        const message = inputElement.value.trim();

        if (!message || isRoomClosed) {
            return;
        }

        try {
            sendHubMessage("SendMessageToRoom", [roomId, message]);
            inputElement.value = "";
            inputElement.focus();
        } catch (error) {
            console.error(error);
            setStatus("전송 실패", "is-disconnected");
        }
    });

    connect().catch((error) => {
        console.error(error);
        setStatus("연결 실패", "is-disconnected");
        setFormEnabled(false);
    });
});
