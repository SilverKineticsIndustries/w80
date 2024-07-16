import React, { memo } from 'react';
import { createUseStyles } from 'react-jss';

const styles = createUseStyles({
    loading: {
        top: "0px",
        position: "fixed",
        left: "calc(50% - 100px / 2)",
        width: "100px"
    }
})

const Spinner = () => {
    const classes = styles();
    return (
        <div className={classes.loading} data-test="spinner">
            <span className="lds-dual-ring"/>
        </div>
    )
}

export default memo(Spinner);