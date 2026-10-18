window.laboratorio = window.laboratorio || {};

laboratorio.setRol = function (rol) {
    sessionStorage.setItem("rol", rol);
};

laboratorio.getRol = function () {
    return sessionStorage.getItem("rol");
};

laboratorio.setToken = function (token) {
    sessionStorage.setItem("token", token);
};

laboratorio.getToken = function () {
    return sessionStorage.getItem("token");
};

laboratorio.removeToken = function () {
    sessionStorage.removeItem("token");
};


window.triggerFileInputClick = (fileInput) => {
    fileInput.click();
};
