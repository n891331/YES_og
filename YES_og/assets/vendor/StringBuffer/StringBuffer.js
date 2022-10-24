function StringBuffer() {
    this.data = [];
}
StringBuffer.prototype.append = function () {
    this.data.push(arguments[0]);
    return this;
}
StringBuffer.prototype.toString = function () {
    return this.data.join("");
}