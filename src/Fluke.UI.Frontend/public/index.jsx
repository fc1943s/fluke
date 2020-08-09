import React, {useState, useEffect} from 'react';
import ReactDOM from 'react-dom';

const Counter = () => {
    const [counter, setCounter] = useState(0);

    useEffect(() => {
        const timer = setInterval(() => {
            setCounter(previousCounter => previousCounter + 1);
        }, 100);

        return () => {
            clearInterval(timer);
        };
    }, []);

    return <span color="green">{counter}</span>;
};

const interval = setInterval(() => {
    const container = document.getElementById("test1");
    if (container) {
        clearInterval(interval);
        ReactDOM.render(<Counter/>, container);
    }
}, 100);

