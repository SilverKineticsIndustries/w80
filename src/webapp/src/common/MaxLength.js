import React, { memo } from 'react';
import { createUseStyles } from 'react-jss'

const styles = createUseStyles({
    wrapper: {
        fontSize: ".75em",
        verticalAlign: "text-top",
        color: "var(--error-text)",
        paddingLeft: "4px"
    }
})

const MaxLength = ({val, max}) => {
    const classes = styles();
    const threshold =  (val || 0).length / Number(max) > 0.75;

    return (
        <React.Fragment>
        {threshold &&
            <span className={classes.wrapper}>
                {(val || 0).length}/{max}
            </span>
        }
        </React.Fragment>
    )
}

export default memo(MaxLength);