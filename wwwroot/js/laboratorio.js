window.laboratorio = window.laboratorio || {};

laboratorio.setRol = function (rol) {
    localStorage.setItem("rol", rol);
};

laboratorio.getRol = function () {
    return localStorage.getItem("rol");
};

laboratorio.setToken = function (token) {
    localStorage.setItem("token", token);
};

laboratorio.getToken = function () {
    return localStorage.getItem("token");
};

laboratorio.removeToken = function () {
    localStorage.removeItem("token");
};


window.triggerFileInputClick = (fileInput) => {
    fileInput.click();
};
