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
    const len = (val || "").length;
    const threshold =  len / Number(max) > 0.75;

    return (
        <>
        {max && threshold &&
            <span className={classes.wrapper}>
                {len}/{max}
            </span>
        }
        </>
    )
}

export default memo(MaxLength);