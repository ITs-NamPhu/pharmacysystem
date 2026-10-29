import { useState, useEffect } from 'react';
import { MdAddCircle } from "react-icons/md";
import ModalCreateManufacturer from './ModalCreateManufacturer';
import ModalUpdateManufacturer from './ModalUpdateManufacturer';
import ModalDeleteManufacturer from './ModalDeleteManufacturer';
import TableManufacturer from './tableManufacturer';
import './IndexManageManufacturer.scss';
import { getAllManufacturerPag } from '../../services/apiService'

const IndexManageManufacturer = (props) => {
    const [listManufacturer, setListManufacturer] = useState()
    const [limitPage, setLimitPage] = useState(10);
    const [pageCount, setPageCount] = useState(0);
    const [currentPage, setCurrentPage] = useState(1);

    const [showModalCreateManufacturer, setShowModalCreateManufacturer] = useState(false);

    const [showModalUpdateManufacturer, setShowModalUpdateManufacturer] = useState(false);
    const [dataUpdate, setDataUpdate] = useState({});

    const [showModalDeleteManufacturer, setShowModalDeleteManufacturer] = useState(false);
    const [dataDelete, setDataDelete] = useState({});

    useEffect(() => {
        fetchListManufacturer(1)
    }, [])

    const fetchListManufacturer = async (page) => {
        let res = await getAllManufacturerPag(page, limitPage);
        console.log("res_manu", res)
        if (res.ec === 0) {
            setListManufacturer(res.dt.manufacturers);
            setPageCount(res.dt.totalPage);
        }
    }

    const handleCreateManufacturer = () => {
        setShowModalCreateManufacturer(!showModalCreateManufacturer);
    }
    const handleUpdateManufacturer = (manufacturer) => {
        setShowModalUpdateManufacturer(!showModalUpdateManufacturer);
        setDataUpdate(manufacturer);
    }
    const handleDeleteManufacturer = (manufacturer) => {
        setShowModalDeleteManufacturer(!showModalDeleteManufacturer);
        setDataDelete(manufacturer);
    }

    return (
        <div className='manage-container'>
            <div className="title">
                Management Manufacturer
            </div>
            <div className='user-content'>
                <div className='btn-add-new'>
                    <button className='btn btn-primary' onClick={() => handleCreateManufacturer()}>
                        <MdAddCircle /> Add Manufacturer
                    </button>
                </div>
                <div className='table-container'>
                    <TableManufacturer
                        fetchListManufacturer={fetchListManufacturer}
                        listManufacturer={listManufacturer}
                        handleUpdateManufacturer={handleUpdateManufacturer}
                        handleDeleteManufacturer={handleDeleteManufacturer}
                        pageCount={pageCount}
                        currentPage={currentPage}
                        setCurrentPage={setCurrentPage} />
                </div>
                <ModalCreateManufacturer
                    show={showModalCreateManufacturer} setShow={setShowModalCreateManufacturer}
                    fetchListManufacturer={fetchListManufacturer}
                />
                <ModalUpdateManufacturer
                    show={showModalUpdateManufacturer} setShow={setShowModalUpdateManufacturer}
                    fetchListManufacturer={fetchListManufacturer}
                    dataUpdate={dataUpdate}
                />
                <ModalDeleteManufacturer
                    show={showModalDeleteManufacturer} setShow={setShowModalDeleteManufacturer}
                    fetchListManufacturer={fetchListManufacturer}
                    dataDelete={dataDelete}
                    setCurrentPage={setCurrentPage}
                />
            </div>
        </div>
    );
}
export default IndexManageManufacturer;
