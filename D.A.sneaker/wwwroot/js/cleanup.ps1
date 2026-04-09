$lines = Get-Content -Encoding UTF8 'layout.js'
$total = $lines.Count
Write-Host "Total: $total"

# Keep lines 1..525 (before broken openChat) and 528..end (skip the broken "function openChat() {" line at 527, index 526)
# Then insert correct openChat/closeChat + keep rest 528 onward
$before = $lines[0..524]  # lines 1-525 (index 0-524)
$after  = $lines[527..($total-1)]  # lines 528-end (index 527+)

$insert = @(
'',
'  // Toggle chat',
'  function openChat() {',
'    isOpen = true;',
'    panel.classList.add(''show'');',
'    aiBtn.classList.add(''open'');',
'    aiBtn.setAttribute(''aria-expanded'', ''true'');',
'    panel.setAttribute(''aria-hidden'', ''false'');',
'    badge.style.display = ''none'';',
'    input.focus();',
'  }',
'',
'  function closeChat() {',
'    isOpen = false;',
'    panel.classList.remove(''show'');',
'    aiBtn.classList.remove(''open'');',
'    aiBtn.setAttribute(''aria-expanded'', ''false'');',
'    panel.setAttribute(''aria-hidden'', ''true'');',
'  }',
'',
'  aiBtn.addEventListener(''click'', () => isOpen ? closeChat() : openChat());',
'  closeBtn.addEventListener(''click'', closeChat);',
'  if (clearBtn) clearBtn.addEventListener(''click'', () => {',
'    if (confirm(''Xoa toan bo lich su chat?'')) clearHistory();',
'  });',
'',
'  document.addEventListener(''keydown'', e => {',
'    if (e.key === ''Escape'' && isOpen) closeChat();',
'  });',
'',
'  if (!loadHistory()) {',
'    renderMsg(''Xin chao! ??? To la <strong>SPARK AI</strong>, tro ly mua sam thong minh cua ban.<br>To co the giup ban tim giay, tu van size, xem uu dai...'', ''bot'', false);',
'  }',
'',
'  function showTyping() {',
'    const div = document.createElement(''div'');',
'    div.className = ''ai-msg bot'';',
'    div.id = ''aiTyping'';',
'    div.innerHTML = `<div class="bot-icon">???</div><div class="bubble" style="padding:8px 14px"><div class="ai-typing"><span></span><span></span><span></span></div></div>`;',
'    messages.appendChild(div);',
'    messages.scrollTop = messages.scrollHeight;',
'  }',
'',
'  function removeTyping() {',
'    const t = document.getElementById(''aiTyping'');',
'    if (t) t.remove();',
'  }',
''
)

$result = $before + $insert + $after
Set-Content -Encoding UTF8 'layout.js' $result
Write-Host "Done. New total: $($result.Count)"
