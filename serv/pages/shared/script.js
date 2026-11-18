function assert(value) {
    if (value === null || value === undefined) {
        throw Error("null or undefined value is not expected here.");
    }
    return value;
}