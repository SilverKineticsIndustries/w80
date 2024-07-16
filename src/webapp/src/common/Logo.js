import React, {memo} from 'react';
import { createUseStyles } from 'react-jss'

const styles = createUseStyles({
    wrapper: {
        marginLeft: "10px",
        marginRight: "10px"
    }
})

const Logo = () => {
    const classes = styles();
    return (
<div className={classes.wrapper}>
    <pre>
██╗    ██╗ █████╗  ██████╗<br/>
██║    ██║██╔══██╗██╔═████╗<br/>
██║ █╗ ██║╚█████╔╝██║██╔██║<br/>
██║███╗██║██╔══██╗████╔╝██║<br/>
╚███╔███╔╝╚█████╔╝╚██████╔╝<br/>
 ╚══╝╚══╝  ╚════╝  ╚═════╝<br/>
    </pre>
 </div>
    )
}

export default memo(Logo);