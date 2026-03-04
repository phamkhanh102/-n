
    document.getElementById("msg")
    .addEventListener("keypress", function (e) {
                if (e.key === "Enter") send();
            });

    async function send() {

            const input = document.getElementById("msg");
    const text = input.value;

    if (!text) return;

    const messages = document.getElementById("messages");

    // USER
    const user = document.createElement("div");
    user.className = "user";
    user.innerText = text;
    messages.appendChild(user);

    input.value = "";

    // TYPING
    const typing = document.createElement("div");
    typing.className = "bot";
    typing.innerText = "AI đang suy nghĩ...";
    messages.appendChild(typing);

    messages.scrollTop = messages.scrollHeight;

    //--------------------------------
    // CALL API
    //--------------------------------

    const res = await fetch("/api/chat", {
        method: "POST",
    headers: {'Content-Type': 'application/json' },
    body: JSON.stringify(text)
            });

    const data = await res.json();

    typing.remove();

    const bot = document.createElement("div");
    bot.className = "bot";

    //--------------------------------
    // TEXT
    //--------------------------------

    if (data.type === "text") {
        bot.innerText = data.message;
            }

    //--------------------------------
    // PRODUCT
    //--------------------------------

    if (data.type === "products") {

        bot.innerHTML = data.data.map(p => `

                        <div class="card">
                            <b>${p.name}</b>
                            <p>${p.brand}</p>

                            <p class="price">
                                 ${p.price.toLocaleString()} VND
                            </p>

                            <p> Còn ${p.stock}</p>
                        </div>

                    `).join("");
            }

    messages.appendChild(bot);
    messages.scrollTop = messages.scrollHeight;
        }

function toggleBox() {
    const box = document.getElementById('box');
    if (box.style.display === 'none') {
        box.style.display = 'block';
    } else {
        box.style.display = 'none';
    }
}