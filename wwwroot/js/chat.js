// Biến toàn cục
const currentUserId = window.currentUserId || "";
const currentUserName = window.currentUserName || "";
const openedThreadIds = new Set();


// Kết nối SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .configureLogging(signalR.LogLevel.Information)
    .build();


connection.on("ReceiveMessage", (userId, message, threadId, senderName, senderAvatarFileName) => {
    const box = document.getElementById(`messages-${threadId}`);
    const now = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    const div = document.createElement("div");

    const isMe = userId === currentUserId;

    let content = "";
    if (message.startsWith("[img]") && message.endsWith("[/img]")) {
        const fileName = message.replace("[img]", "").replace("[/img]", "");
        content = `<img src="/uploads/chat/${fileName}" class="img-fluid rounded" style="max-width: 150px;" />`;
    } else {
        content = parseEmojis(message);
    }

    if (isMe) {
        div.innerHTML = `
            <div class="text-end text-primary">
                <div>${content}</div>
                <small class="text-muted">${now}</small>
            </div>`;
    } else {
        div.innerHTML = `
            <div class="d-flex align-items-start mb-2">
                <img src="/uploads/avatar/${senderAvatarFileName}" class="rounded-circle me-2" style="width: 32px; height: 32px;" alt="Avatar" />
                <div class="bg-light p-2 rounded border">
                    <div>${content}</div>
                    <small class="text-muted">${now}</small>
                </div>
            </div>`;
    }

    if (box) {
        box.appendChild(div);
        box.scrollTop = box.scrollHeight;
    }

    const popup = document.getElementById("chatListPopup");
    const badge = document.getElementById("chatNotificationBadge");
    if (popup && popup.style.display === "none" && badge) {
        badge.style.display = "inline";
    }

    if (!openedThreadIds.has(threadId)) {
        const item = document.querySelector(`#chatThreads .chat-thread[data-id='${threadId}']`);
        if (item && !item.querySelector(".unread-indicator")) {
            const dot = document.createElement("span");
            dot.className = "unread-indicator text-danger ms-2";
            dot.innerHTML = "🔴";
            item.appendChild(dot);
        }
    }
});


// Nhận sự kiện đang nhập
connection.on("ReceiveTyping", (threadId, userName) => {
    const box = document.getElementById(`messages-${threadId}`);
    if (!box) return;

    let typingId = `typing-${threadId}`;
    let existing = document.getElementById(typingId);
    if (existing) {
        clearTimeout(existing.dataset.timeout);
    } else {
        const typingDiv = document.createElement("div");
        typingDiv.id = typingId;
        typingDiv.className = "text-muted fst-italic";
        typingDiv.textContent = `${userName} đang nhập...`;
        box.appendChild(typingDiv);
    }

    const timeoutId = setTimeout(() => {
        const el = document.getElementById(typingId);
        if (el) el.remove();
    }, 3000);
    document.getElementById(typingId).dataset.timeout = timeoutId;
});

// Bắt đầu kết nối
connection.start()
    .then(() => console.log("✅ Connected to SignalR"))
    .catch(err => console.error("❌ Failed to connect SignalR", err));

// Gửi tin nhắn
function sendMessage(e, threadId) {
    if (e.key === "Enter") {
        const input = e.target;
        let msg = input.value.trim();
        msg = parseEmojis(msg);
        if (!msg) return;

        connection.invoke("SendMessage", threadId, currentUserId, msg)
            .catch(err => console.error("SendMessage Error:", err));

        input.value = "";
    }
}



// Gửi trạng thái đang nhập
function notifyTyping(threadId) {
    connection.invoke("SendTyping", threadId, currentUserId, currentUserName)
        .catch(err => console.error("SendTyping Error:", err));
}

// Mở popup danh sách chat
document.getElementById("chatIcon").addEventListener("click", function (e) {
    e.preventDefault();
    const popup = document.getElementById("chatListPopup");
    const badge = document.getElementById("chatNotificationBadge");

    if (popup.style.display === "none") {
        popup.style.display = "block";
        if (badge) badge.style.display = "none";
    } else {
        popup.style.display = "none";
    }

    fetch("/Chat/GetThreads")
        .then(res => res.json())
        .then(data => {
            const container = document.getElementById("chatThreads");
            container.innerHTML = "";
            data.forEach(thread => {
                const item = document.createElement("div");
                item.innerHTML = `<b>${thread.title}</b>${thread.unreadCount > 0 ? ` <span class="badge bg-danger">${thread.unreadCount}</span>` : ""}`;
                item.className = "chat-thread p-1 border-bottom cursor-pointer";
                item.dataset.id = thread.id;
                item.onclick = () => openChatBox(thread.id, thread.title);
                container.appendChild(item);
            });
        });
});

// Mở khung chat cụ thể
function openChatBox(threadId, title) {
    const container = document.getElementById("chatBoxesContainer");

    if (document.getElementById("chatBox-" + threadId)) return;
    window.lastThreadId = threadId;

    const chatBox = document.createElement("div");
    chatBox.className = "card mb-2";
    chatBox.style.width = "300px";
    chatBox.id = "chatBox-" + threadId;
    chatBox.innerHTML = `
        <div class="card-header d-flex justify-content-between">
            <strong>${title}</strong>
            <button onclick="this.closest('.card').remove()">×</button>
        </div>
        <div class="card-body overflow-auto" style="height: 200px;" id="messages-${threadId}"></div>
        <div class="card-footer position-relative">
            <div class="input-group">
                <input type="text" class="form-control" placeholder="Aa"
                    onkeydown="sendMessage(event, '${threadId}')" 
                    oninput="notifyTyping('${threadId}')"
                    id="msg-${threadId}" />

                <input type="file" accept="image/*" style="display: none" id="imgInput-${threadId}" onchange="uploadImage(this, '${threadId}')" />
                <button class="btn btn-outline-secondary" onclick="document.getElementById('imgInput-${threadId}').click()">📷</button>
                <button class="btn btn-outline-secondary" onclick="toggleEmojiPicker('${threadId}')">😀</button>
            </div>
            <div id="emojiPicker-${threadId}" class="border p-2 rounded bg-light mt-2"
                style="display:none; max-height: 120px; overflow-y: auto; position: absolute; bottom: 50px; right: 10px; z-index: 1000;">
            </div>
        </div>
    `;
    container.appendChild(chatBox);

    // ✅ Join group chat
    connection.invoke("JoinThread", threadId.toString())
        .then(() => console.log("✅ JoinThread OK"))
        .catch(err => console.error("❌ JoinThread Error (client):", err));

    // ✅ Mark as read
    fetch(`/Chat/MarkAsRead?threadId=${threadId}`, {
        method: "POST"
    });

    // ✅ Bỏ hiển thị "chưa đọc"
    openedThreadIds.add(threadId);
    const threadItem = document.querySelector(`#chatThreads .chat-thread[data-id='${threadId}']`);
    if (threadItem) {
        const badge = threadItem.querySelector(".unread-indicator");
        if (badge) badge.remove();
    }

    // ✅ Load lại tin nhắn từ DB
    fetch(`/Chat/GetMessages?threadId=${threadId}`)
        .then(res => res.json())
        .then(data => {
            const box = document.getElementById("messages-" + threadId);
            box.innerHTML = "";

            data.forEach(msg => {
                const div = document.createElement("div");
                const isMe = msg.senderId == currentUserId;
                let content = "";

                if (msg.content.startsWith("[img]") && msg.content.endsWith("[/img]")) {
                    const fileName = msg.content.replace("[img]", "").replace("[/img]", "");
                    content = `<img src="/uploads/chat/${fileName}" class="img-fluid rounded" style="max-width: 150px;" />`;
                } else {
                    content = parseEmojis(msg.content);
                }

                if (isMe) {
                    div.innerHTML = `
                        <div class="text-end text-primary">
                            <div>${content}</div>
                            <small class="text-muted">${msg.time}</small>
                        </div>`;
                } else {
                    div.innerHTML = `
                        <div class="d-flex align-items-start mb-2">
                            <img src="/uploads/avatar/${msg.avatar}" class="rounded-circle me-2" style="width: 32px; height: 32px;" />
                            <div class="bg-light p-2 rounded border">
                                <div>${content}</div>
                                <small class="text-muted">${msg.time}</small>
                            </div>
                        </div>`;
                }

                box.appendChild(div);
                box.scrollTop = box.scrollHeight;
            });
        });

    renderEmojiPicker(threadId);
}


// Bắt đầu chat từ hồ sơ người dùng
function startChat(receiverId, receiverName) {
    fetch(`/Chat/StartThread?receiverId=${receiverId}`)
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                openChatBox(data.threadId, receiverName);
            } else {
                alert("Không thể bắt đầu đoạn chat.");
            }
        })
        .catch(err => {
            console.error("Lỗi khi bắt đầu chat:", err);
            alert("Có lỗi xảy ra khi gửi yêu cầu.");
        });
}

function parseEmojis(text) {
    const emojiMap = {
        ":grinning:": "😀",
        ":smiley:": "😃",
        ":smile:": "😄",
        ":grin:": "😁",
        ":laughing:": "😆",
        ":sweat_smile:": "😅",
        ":joy:": "😂",
        ":rofl:": "🤣",
        ":blush:": "😊",
        ":innocent:": "😇",
        ":slightly_smile:": "🙂",
        ":upside_down:": "🙃",
        ":wink:": "😉",
        ":heart_eyes:": "😍",
        ":kissing_heart:": "😘",
        ":stuck_out_tongue_winking_eye:": "😜",
        ":zany_face:": "🤪",
        ":sunglasses:": "😎",
        ":star_struck:": "🤩",
        ":cry:": "😢",
        ":sob:": "😭",
        ":rage:": "😡",
        ":angry:": "😠",
        ":face_with_symbols_over_mouth:": "🤬",
        ":thumbsup:": "👍",
        ":thumbsdown:": "👎",
        ":raised_hands:": "🙌",
        ":clap:": "👏",
        ":muscle:": "💪",
        ":heart:": "❤️",
        ":broken_heart:": "💔",
        ":fire:": "🔥",
        ":100:": "💯",
        ":tada:": "🎉",
        ":birthday:": "🎂",
        ":star:": "🌟"
    };

    for (const key in emojiMap) {
        text = text.replaceAll(key, emojiMap[key]);
    }
    return text;
}

// Gửi ảnh an toàn qua SignalR bằng fileName
function uploadImage(input, threadId) {
    const file = input.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    fetch("/Chat/UploadChatImage", {
        method: "POST",
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const fileName = data.fileName;
                if (connection.state === signalR.HubConnectionState.Connected) {
                    connection.invoke("SendMessage", threadId, currentUserId, `[img]${fileName}[/img]`)
                        .catch(err => console.error("SendMessage Image Error:", err));
                } else {
                    console.warn("⚠ Chưa kết nối SignalR");
                }
            } else {
                alert("Tải ảnh thất bại");
            }
        });
}
function toggleEmojiPicker(threadId) {
    const picker = document.getElementById(`emojiPicker-${threadId}`);
    picker.style.display = picker.style.display === "none" ? "block" : "none";
}

document.body.addEventListener("click", function (e) {
    if (!e.target.closest(".card-footer")) {
        document.querySelectorAll("[id^='emojiPicker-']").forEach(p => p.style.display = "none");
    }
});


function renderEmojiPicker(threadId) {
    const emojis = "😀 😃 😄 😁 😆 😅 😂 🤣 😊 😇 🙂 🙃 😉 😍 😘 😜 🤪 😎 🤩 😢 😭 😡 😠 🤬 👍 👎 🙌 👏 💪 ❤️ 💔 🔥 💯 🎉 🎂 🌟".split(" ");
    const picker = document.getElementById(`emojiPicker-${threadId}`);
    picker.innerHTML = "";
    emojis.forEach(e => {
        const span = document.createElement("span");
        span.className = "emoji me-1";
        span.textContent = e;
        span.style.cursor = "pointer";
        span.onclick = (event) => {
            event.stopPropagation(); // Ngăn click gây hiệu ứng ngoài ý muốn
            const input = document.getElementById(`msg-${threadId}`);
            input.value += e; // ✅ Chỉ chèn emoji
            input.focus(); // Không gửi tin nhắn
            notifyTyping(threadId);
        };
        picker.appendChild(span);
    });
}



