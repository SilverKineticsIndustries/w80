import React from 'react';

const ModalWrapper = ({children}) => {
    return (
        <div className="overlay">
            <div className="modal">
                { children }
            </div>
        </div>
    )
}

export default ModalWrapper;