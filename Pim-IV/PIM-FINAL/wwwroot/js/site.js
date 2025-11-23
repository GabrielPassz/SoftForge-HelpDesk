// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Global site script: list/table limiting + login cache/auto logout
(function(){
 // ---- Utilities ----
 function onReady(fn){
 if(document.readyState === 'loading') document.addEventListener('DOMContentLoaded', fn); else fn();
 }
 function makeButton(text, cls){
 const btn = document.createElement('button');
 btn.type = 'button';
 btn.className = cls || 'btn btn-sm btn-secondary';
 btn.textContent = text || 'Mostrar mais';
 return btn;
 }

 // ---- Login cache + auto logout ----
 // Requisito: "salvar o login e deslogar depois de um minuto FORA da página (tem que sair dela)".
 // Estratégia:
 // - Ao autenticar (qualquer página autenticada carrega este script com body[data-auth=true]) marcamos sessionStorage flag.
 // - Mantemos timestamp da última vez que estivemos em página autenticada.
 // - Se o usuário navega para página anônima (ex: /Site/Login, /Site/Cadastro etc) OU fecha/oculta a aba por >60s, executamos logout.
 // - "Fora da página" interpretado como: aba não visível OU URL não autenticada.
 const LOG_KEY = 'auth.active';
 const LAST_SEEN_KEY = 'auth.lastSeen';
 const GRACE_MS =60000; //1 minuto
 const ANON_PATHS = ['/site/login','/site/cadastro','/site/recuperarsenha','/site/validarcodigoredefinir'];

 function isAuthenticated(){
 return document.body.getAttribute('data-auth') === 'True' || document.body.getAttribute('data-auth') === 'true';
 }
 function now(){ return Date.now(); }
 function updateLastSeen(){ sessionStorage.setItem(LAST_SEEN_KEY, String(now())); }
 function pathLower(){ return (location.pathname||'').toLowerCase(); }
 function isAnonPage(){
 const p = pathLower();
 return ANON_PATHS.some(a => p.startsWith(a));
 }
 async function performLogout(){
 try {
 // Post para /Site/Logout
 await fetch('/Site/Logout', { method: 'POST', credentials: 'include' });
 } catch(e){ console.warn('Falha logout', e); }
 // Limpa flags e força redirecionar para login
 sessionStorage.removeItem(LOG_KEY);
 sessionStorage.removeItem(LAST_SEEN_KEY);
 if(!isAnonPage()) location.href = '/Site/Login';
 }
 function checkTimeout(){
 const active = sessionStorage.getItem(LOG_KEY) === '1';
 if(!active) return; // nada a fazer
 const last = parseInt(sessionStorage.getItem(LAST_SEEN_KEY)||'0',10);
 if(!isAuthenticated()){
 // estado atual não autenticado porém cache ativo: invalida
 performLogout();
 return;
 }
 if(document.hidden){
 // Aba oculta: verificar se passou tempo
 if(last && now() - last > GRACE_MS){
 performLogout();
 }
 } else {
 // Aba visível em página autenticada: atualiza timestamp
 updateLastSeen();
 }
 }
 function initAuthCache(){
 if(isAuthenticated()){
 if(sessionStorage.getItem(LOG_KEY) !== '1'){
 sessionStorage.setItem(LOG_KEY,'1');
 }
 updateLastSeen();
 } else {
 // Em página anônima: se cache ativo e passou limite desde último visto em página protegida -> logout
 const last = parseInt(sessionStorage.getItem(LAST_SEEN_KEY)||'0',10);
 if(sessionStorage.getItem(LOG_KEY) === '1' && last && now() - last > GRACE_MS){
 performLogout();
 } else {
 // Caso contrário apenas não atualiza (permite voltar rápido)
 }
 }
 document.addEventListener('visibilitychange', checkTimeout);
 window.addEventListener('focus', checkTimeout);
 // Checagem periódica de segurança
 setInterval(checkTimeout,5000);
 // Ao sair da página autenticada (navegação) marcamos último timestamp para iniciar contagem
 window.addEventListener('beforeunload', function(){
 if(isAuthenticated()) updateLastSeen();
 });
 }

 // ---- Limit rows in lists/tables (merged from limitLists.js) ----
 const DEFAULT_LIMIT =3;
 const DEFAULT_TABLE_COUNT_LIMIT =3;
 function readLimit(el){ const n = parseInt(el.getAttribute('data-limit'),10); return Number.isFinite(n)&&n>0?n:DEFAULT_LIMIT; }
 function readTableCountLimit(){ const n = parseInt(document.body.getAttribute('data-max-tables'),10); return Number.isFinite(n)&&n>0?n:DEFAULT_TABLE_COUNT_LIMIT; }

 function applyToList(list){
 const limit = readLimit(list);
 const items = Array.from(list.children).filter(n=>n.nodeType===1);
 if(items.length <= limit) return;
 const hidden = items.slice(limit);
 hidden.forEach(li=>li.classList.add('is-hidden'));
 const btn = makeButton('Mostrar mais','btn btn-sm btn-secondary limit-toggle');
 btn.addEventListener('click',()=>{
 const expanded = hidden[0].classList.contains('is-hidden')===false;
 hidden.forEach(li=>li.classList.toggle('is-hidden', expanded));
 btn.textContent = expanded? 'Mostrar mais':'Mostrar menos';
 });
 const wrap = document.createElement('div');
 wrap.appendChild(btn);
 list.parentNode.insertBefore(wrap, list.nextSibling);
 }

 function applyToTable(table){
 const limit = readLimit(table);
 const tbodies = table.tBodies && table.tBodies.length ? Array.from(table.tBodies) : [table];
 const rows = tbodies.flatMap(tb=>Array.from(tb.querySelectorAll('tr')));
 if(rows.length <= limit) return;
 const hidden = rows.slice(limit);
 hidden.forEach(r=>r.classList.add('is-hidden'));
 const btn = makeButton('Mostrar mais','btn btn-sm btn-secondary limit-toggle');
 btn.addEventListener('click',()=>{
 const expanded = hidden[0].classList.contains('is-hidden')===false;
 hidden.forEach(r=>r.classList.toggle('is-hidden', expanded));
 btn.textContent = expanded? 'Mostrar mais':'Mostrar menos';
 });
 const wrap = document.createElement('div');
 wrap.className = 'limit-wrap';
 wrap.style.marginTop = '4px';
 wrap.appendChild(btn);
 table.parentNode.insertBefore(wrap, table.nextSibling);
 }

 function applyTableCountLimit(){
 const limit = readTableCountLimit();
 const tables = Array.from(document.querySelectorAll('table.limit-table'));
 if(tables.length <= limit) return;
 const hiddenSections = tables.slice(limit).map(t=>t.closest('.db-section')||t).filter(Boolean);
 hiddenSections.forEach(sec=>sec.classList.add('is-hidden'));
 const btn = makeButton('Mostrar mais tabelas','btn btn-sm btn-secondary limit-toggle');
 btn.addEventListener('click',()=>{
 const collapsed = hiddenSections[0].classList.contains('is-hidden')===false;
 hiddenSections.forEach(sec=>sec.classList.toggle('is-hidden', collapsed));
 btn.textContent = collapsed? 'Mostrar mais tabelas':'Mostrar menos tabelas';
 });
 const anchorSec = (tables[limit-1].closest('.db-section') || tables[limit-1]);
 const wrap = document.createElement('div');
 wrap.className = 'limit-table-wrap';
 wrap.style.margin = '8px0';
 wrap.appendChild(btn);
 anchorSec.parentNode.insertBefore(wrap, anchorSec.nextSibling);
 }

 // ---- Inline scripts migrated from views ----
 function attachCadastroValidation(){
 const form = document.getElementById('cadForm');
 if(!form) return;
 form.addEventListener('submit', function(e){
 const senha = document.getElementById('senha');
 const confirma = document.getElementById('confirma_senha');
 if(!senha || !confirma) return;
 if(senha.value !== confirma.value){
 e.preventDefault();
 alert('As senhas não coincidem.');
 }
 });
 }

 function attachRegistroValidation(){
 const form = document.getElementById('registroForm');
 if(!form) return;
 form.addEventListener('submit', function(e){
 const senha = document.getElementById('senha');
 const confirma = document.getElementById('confirma_senha');
 if(!senha || !confirma) return;
 if(senha.value !== confirma.value){
 e.preventDefault();
 alert('As senhas não coincidem.');
 }
 });
 }

 function attachNotifToggle(){
 const btn = document.getElementById('toggleNotif');
 const rest = document.getElementById('notifRest');
 if(!btn || !rest) return;
 btn.addEventListener('click', function(){
 const visible = rest.style.display !== 'none';
 rest.style.display = visible ? 'none' : 'block';
 btn.textContent = visible ? 'Mostrar mais' : 'Mostrar menos';
 });
 }

 function restoreMeusChamadosFilters(){
 const statusSel = document.getElementById('filtro_status');
 const prioridadeSel = document.getElementById('filtro_prioridade');
 // values can be rendered by server via data-* attributes (if added) or fallback to inline hidden inputs if needed
 const form = statusSel && statusSel.form;
 if(statusSel){
 const v = statusSel.getAttribute('data-current') || (form? form.getAttribute('data-current-status'):null) || window.MeusChamadosStatus;
 if(v){ for(let i=0;i<statusSel.options.length;i++){ if(statusSel.options[i].value===v){ statusSel.selectedIndex=i; break; } } }
 }
 if(prioridadeSel){
 const v = prioridadeSel.getAttribute('data-current') || (form? form.getAttribute('data-current-prioridade'):null) || window.MeusChamadosPrioridade;
 if(v){ for(let i=0;i<prioridadeSel.options.length;i++){ if(prioridadeSel.options[i].value===v){ prioridadeSel.selectedIndex=i; break; } } }
 }
 }

 onReady(()=>{
 // hidden css for toggle
 if(!document.getElementById('limitlists-hidden-style')){
 const style = document.createElement('style');
 style.id = 'limitlists-hidden-style';
 style.textContent = '.is-hidden{display:none !important}';
 document.head.appendChild(style);
 }
 // init auth cache
 initAuthCache();
 // apply limiters
 document.querySelectorAll('ul.limit-rows:not(.no-limit), ol.limit-rows:not(.no-limit)')
 .forEach(applyToList);
 document.querySelectorAll('table.limit-rows:not(.no-limit)')
 .forEach(applyToTable);
 applyTableCountLimit();
 // migrated scripts
 attachCadastroValidation();
 attachRegistroValidation();
 attachNotifToggle();
 restoreMeusChamadosFilters();
 });
})();
