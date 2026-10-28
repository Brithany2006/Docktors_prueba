// Comportamiento visual de la marca Klinic: encabezado que se achica, aparicion al hacer scroll y ventana de pasos.
window.klinic = (function () {
    var io = null;
    var preferenciaSistema = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    // Sin movimiento: lo pide el sistema operativo o el visitante pulso "Pausar animaciones" (clase kl-quieto en <html>)
    function quieto() { return preferenciaSistema || document.documentElement.classList.contains('kl-quieto'); }

    function encabezado() {
        var h = document.querySelector('.kl-header');
        if (h) h.classList.toggle('kl-scrolled', window.scrollY > 30);
    }
    window.addEventListener('scroll', encabezado, { passive: true });

    // Marca como visibles los elementos .kl-reveal cuando entran en pantalla (escalonados).
    function reveal() {
        var pendientes = document.querySelectorAll('.kl-reveal:not(.kl-on)');
        if (!pendientes.length) return;
        if (quieto() || !('IntersectionObserver' in window)) {
            pendientes.forEach(function (el) { el.classList.add('kl-on'); });
            return;
        }
        if (!io) {
            io = new IntersectionObserver(function (entradas) {
                entradas.forEach(function (e) {
                    if (e.isIntersecting) { e.target.classList.add('kl-on'); io.unobserve(e.target); }
                });
            }, { threshold: 0.15 });
        }
        pendientes.forEach(function (el, i) {
            el.style.transitionDelay = (i % 4) * 90 + 'ms';
            io.observe(el);
        });
        encabezado();
    }

    // Ventana "Como agendar por el IESS": los pasos entran uno por uno y la linea se va pintando.
    function abrirDialogo(id) {
        var dlg = document.getElementById(id);
        if (!dlg || typeof dlg.showModal !== 'function') return;
        if (!dlg.dataset.cierraAfuera) {
            dlg.addEventListener('click', function (e) { if (e.target === dlg) dlg.close(); });
            dlg.dataset.cierraAfuera = '1';
        }
        if (!dlg.open) dlg.showModal();
        var items = Array.prototype.slice.call(dlg.querySelectorAll('li'));
        var fill = dlg.querySelector('.kl-fill');
        if (fill) fill.style.height = '0';
        items.forEach(function (li) { li.classList.remove('kl-on'); });
        items.forEach(function (li, i) {
            setTimeout(function () {
                li.classList.add('kl-on');
                if (fill) fill.style.height = li.offsetTop + 'px';
            }, quieto() ? 0 : 150 + i * 140);
        });
    }

    function cerrarDialogo(id) {
        var dlg = document.getElementById(id);
        if (dlg && dlg.open) dlg.close();
    }

    function irA(id) {
        var el = document.getElementById(id);
        if (el) el.scrollIntoView({ behavior: quieto() ? 'auto' : 'smooth', block: 'start' });
    }

    function reducirMovimiento() { return quieto(); }

    // Enlace "Saltar al contenido": un href="#..." chocaria con <base href="/"> de Blazor, por eso se hace con foco
    function saltar(id) {
        var el = document.getElementById(id);
        if (!el) return;
        el.focus({ preventScroll: true });
        el.scrollIntoView({ behavior: 'auto', block: 'start' });
    }

    // Boton "Pausar animaciones" del pie: apaga las animaciones decorativas que no paran (WCAG 2.2.2). Se recuerda en el navegador.
    function animacionesPausadas() { return document.documentElement.classList.contains('kl-quieto'); }
    function alternarAnimaciones() {
        var pausadas = document.documentElement.classList.toggle('kl-quieto');
        try { localStorage.setItem('kl-quieto', pausadas ? '1' : '0'); } catch (e) { }
        return pausadas;
    }

    return { reveal: reveal, abrirDialogo: abrirDialogo, cerrarDialogo: cerrarDialogo, irA: irA, sinMovimiento: reducirMovimiento,
             saltar: saltar, alternarAnimaciones: alternarAnimaciones, animacionesPausadas: animacionesPausadas };
})();

// Aviso de version nueva. Cada 3 minutos (y al volver a la pestana) se pide al servidor el index.html y el manifiesto de Blazor:
// si el sello <meta name="klinic-version"> o el manifiesto cambiaron, aparece una ventana grande al centro para actualizar. Si la persona no la atiende
// y deja de usar la pagina (3 min con el aviso y 1 min sin tocar nada), la pagina se recarga sola.
(function () {
    var INTERVALO = 3 * 60 * 1000;
    var meta = document.querySelector('meta[name="klinic-version"]');
    var miVersion = meta ? meta.content : '';
    var huellaInicial = null, aviso = null, avisoDesde = 0, ultimaActividad = Date.now();

    function huella(texto) { var h = 5381; for (var i = 0; i < texto.length; i++) h = ((h << 5) + h + texto.charCodeAt(i)) | 0; return String(h); }
    function pedir(url) {
        return fetch(url + '?t=' + Date.now(), { cache: 'no-store' }).then(function (r) { return r.ok ? r.text() : null; });
    }

    function mostrarAviso() {
        if (aviso) return;
        avisoDesde = Date.now();
        // Ventana grande al centro con fondo que tapa toda la pagina (incluido el login): la barra de abajo pasaba desapercibida.
        // Estilos en linea: debe verse bien aunque el navegador tenga un CSS viejo en cache
        aviso = document.createElement('div');
        aviso.style.cssText = 'position:fixed;inset:0;z-index:2147483000;background:rgba(11,46,51,.92);display:flex;align-items:center;justify-content:center;padding:1rem;font-family:Inter,system-ui,sans-serif';
        var caja = document.createElement('div');
        caja.setAttribute('role', 'alertdialog');
        caja.setAttribute('aria-modal', 'true');
        caja.setAttribute('aria-labelledby', 'kl-ver-titulo');
        caja.setAttribute('aria-describedby', 'kl-ver-texto');
        caja.style.cssText = 'background:#fff;color:#0b2e33;border-radius:1.5rem;padding:2.5rem 2rem;max-width:34rem;width:100%;text-align:center;box-shadow:0 24px 64px rgba(0,0,0,.45);border-top:8px solid #c8175d';
        var icono = document.createElement('div');
        icono.setAttribute('aria-hidden', 'true');
        icono.textContent = '↻';
        icono.style.cssText = 'font-size:4rem;line-height:1;color:#c8175d;margin-bottom:.75rem';
        var titulo = document.createElement('h2');
        titulo.id = 'kl-ver-titulo';
        titulo.textContent = 'Hay una versión nueva del sistema';
        titulo.style.cssText = 'font-size:1.9rem;line-height:1.2;font-weight:800;margin:0 0 .75rem;color:#0b2e33';
        var texto = document.createElement('p');
        texto.id = 'kl-ver-texto';
        texto.textContent = 'Debes actualizar la página para ver los últimos cambios y evitar errores. Si tienes algo sin guardar, guárdalo antes de continuar.';
        texto.style.cssText = 'font-size:1.15rem;line-height:1.5;margin:0 0 1.75rem;color:#334e53';
        var boton = document.createElement('button');
        boton.type = 'button';
        boton.textContent = 'Actualizar ahora';
        boton.style.cssText = 'background:#c8175d;color:#fff;border:0;border-radius:999px;padding:1rem 2.5rem;min-height:56px;font:800 1.25rem/1 Inter,system-ui,sans-serif;cursor:pointer;box-shadow:0 8px 20px rgba(200,23,93,.4)';
        boton.onclick = function () { location.reload(); };
        caja.appendChild(icono);
        caja.appendChild(titulo);
        caja.appendChild(texto);
        caja.appendChild(boton);
        aviso.appendChild(caja);
        document.body.appendChild(aviso);
        boton.focus();
        // El foco no sale de la ventana mientras el aviso este abierto (solo hay un boton)
        aviso.addEventListener('keydown', function (e) { if (e.key === 'Tab') { e.preventDefault(); boton.focus(); } });
    }

    function revisar() {
        Promise.all([pedir('/index.html'), pedir('/_framework/blazor.boot.json')]).then(function (r) {
            if (r[0] === null || r[1] === null) return;
            var m = /<meta name="klinic-version" content="([^"]*)"/.exec(r[0]);
            var versionServidor = m ? m[1] : '';
            var h = huella(r[1]);
            if (huellaInicial === null) huellaInicial = h;
            if ((miVersion && versionServidor && versionServidor !== miVersion) || h !== huellaInicial) mostrarAviso();
        }).catch(function () { /* sin red: se reintenta en el siguiente ciclo */ });
    }

    ['keydown', 'pointerdown', 'input'].forEach(function (ev) {
        document.addEventListener(ev, function () { ultimaActividad = Date.now(); }, true);
    });
    document.addEventListener('visibilitychange', function () { if (document.visibilityState === 'visible') revisar(); });
    setInterval(revisar, INTERVALO);
    setInterval(function () {
        if (aviso && Date.now() - avisoDesde > 3 * 60 * 1000 && Date.now() - ultimaActividad > 60 * 1000) location.reload();
    }, 20 * 1000);
    setTimeout(revisar, 8000);

    // Para pruebas: klinic.version.revisar()
    window.klinic.version = { revisar: revisar };
})();

// Cierre diario de sesion del personal (6:00 y 13:00, hora de Ecuador). El token de empleado trae el claim CorteDiario y vence en el corte:
// 5 minutos antes se avisa para que guarden su trabajo y, al llegar la hora, se borra el token y se vuelve al login para no dejar
// datos de pacientes en una pantalla sin atender. El servidor ya rechaza el token vencido; esto solo evita que la pantalla se quede abierta.
(function () {
    var ANTES = 5 * 60 * 1000;
    var corteAvisado = 0, aviso = null;

    function corteDelToken() {
        try {
            var t = localStorage.getItem('token');
            if (!t) return 0;
            var p = JSON.parse(atob(t.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
            return p.CorteDiario && p.exp ? p.exp * 1000 : 0;
        } catch (e) { return 0; }
    }

    function hora(ms) {
        try { return new Date(ms).toLocaleTimeString('es-EC', { timeZone: 'America/Guayaquil', hour: '2-digit', minute: '2-digit', hour12: false }); }
        catch (e) { return ''; }
    }

    function cerrarSesion() {
        try { if (window.laboratorio && laboratorio.removeToken) laboratorio.removeToken(); } catch (e) { }
        location.href = '/login?sesion=expirada';
    }

    function mostrarAviso(corte) {
        if (aviso) return;
        aviso = document.createElement('div');
        aviso.style.cssText = 'position:fixed;inset:0;z-index:2147482000;background:rgba(11,46,51,.85);display:flex;align-items:center;justify-content:center;padding:1rem;font-family:Inter,system-ui,sans-serif';
        var caja = document.createElement('div');
        caja.setAttribute('role', 'alertdialog');
        caja.setAttribute('aria-modal', 'true');
        caja.setAttribute('aria-labelledby', 'kl-cor-titulo');
        caja.setAttribute('aria-describedby', 'kl-cor-texto');
        caja.style.cssText = 'background:#fff;color:#0b2e33;border-radius:1.5rem;padding:2.5rem 2rem;max-width:34rem;width:100%;text-align:center;box-shadow:0 24px 64px rgba(0,0,0,.45);border-top:8px solid #c8175d';
        var titulo = document.createElement('h2');
        titulo.id = 'kl-cor-titulo';
        titulo.textContent = 'Tu sesión se cerrará a las ' + hora(corte);
        titulo.style.cssText = 'font-size:1.9rem;line-height:1.2;font-weight:800;margin:0 0 .75rem;color:#0b2e33';
        var texto = document.createElement('p');
        texto.id = 'kl-cor-texto';
        texto.textContent = 'Por seguridad la sesión se cierra todos los días a las 6:00 y a las 13:00. Guarda tu trabajo: después de esa hora deberás ingresar de nuevo.';
        texto.style.cssText = 'font-size:1.15rem;line-height:1.5;margin:0 0 1.75rem;color:#334e53';
        var boton = document.createElement('button');
        boton.type = 'button';
        boton.textContent = 'Entendido';
        boton.style.cssText = 'background:#c8175d;color:#fff;border:0;border-radius:999px;padding:1rem 2.5rem;min-height:56px;font:800 1.25rem/1 Inter,system-ui,sans-serif;cursor:pointer;box-shadow:0 8px 20px rgba(200,23,93,.4)';
        boton.onclick = function () { aviso.remove(); };
        caja.appendChild(titulo);
        caja.appendChild(texto);
        caja.appendChild(boton);
        aviso.appendChild(caja);
        document.body.appendChild(aviso);
        boton.focus();
        aviso.addEventListener('keydown', function (e) { if (e.key === 'Tab') { e.preventDefault(); boton.focus(); } });
    }

    function revisar() {
        var corte = corteDelToken();
        if (!corte) return;
        var falta = corte - Date.now();
        if (falta <= 0) { cerrarSesion(); return; }
        // Se avisa una sola vez por corte; si la persona lo cierra con "Entendido" no vuelve a salir hasta el siguiente
        if (falta <= ANTES && corteAvisado !== corte) { corteAvisado = corte; mostrarAviso(corte); }
    }

    document.addEventListener('visibilitychange', function () { if (document.visibilityState === 'visible') revisar(); });
    setInterval(revisar, 15 * 1000);
    setTimeout(revisar, 3000);

    // Para pruebas: klinic.sesionDiaria.revisar()
    window.klinic.sesionDiaria = { revisar: revisar };
})();
