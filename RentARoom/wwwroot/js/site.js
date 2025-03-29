// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Global helper functions

/**
 * Debounce function to delay execution until after a specified time.
 * @param {Function} func - Function to debounce.
 * @param {number} delay - Delay in milliseconds.
 * @returns {Function} - Debounced function.
 */
//export function debounce(func, delay) {
//    let timeoutId;
//    return function (...args) {
//        clearTimeout(timeoutId);
//        timeoutId = setTimeout(() => func.apply(this, args), delay);
//    };
//}

// site.js (Modified debounce with logging)
export function debounce(func, delay) {
    let timeoutId;
    return function (...args) {
        //console.log("debounce - Arguments received:", args); // Log the arguments

        const context = this;
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => {
            //console.log("debounce - Executing function with arguments:", args); // Log before execution
            func.apply(context, args);
        }, delay);
    };
}