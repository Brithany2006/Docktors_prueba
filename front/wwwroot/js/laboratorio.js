window.laboratorio = window.laboratorio || {};

laboratorio.setRol = function (rol) {
    localStorage.setItem("rol", rol);
};

laboratorio.getRol = function () {
    return localStorage.getItem("rol");
};

laboratorio.setToken = function (token) {
    // Antes setToken(null) guardaba el texto "null" como si fuera un token
    if (token === null || token === undefined || token === "") localStorage.removeItem("token");
    else localStorage.setItem("token", token);
};

laboratorio.getToken = function () {
    return localStorage.getItem("token");
};

laboratorio.removeToken = function () {
    // "token" lo leen las pantallas y "jwt_token" (TokenStore) lo reaplica Program.cs al cargar: ambos deben salir al cerrar sesion
    localStorage.removeItem("token");
    localStorage.removeItem("jwt_token");
    localStorage.removeItem("rol");
};


window.triggerFileInputClick = (fileInput) => {
    fileInput.click();
};
